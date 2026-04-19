using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Npgsql;

namespace GreenStock2
{
    /// <summary>
    /// Сервис обработки сроков годности.
    /// — Списывает просроченные партии (stock_batches) и фиксирует убыток в write_offs.
    /// — Устанавливает скидку 20% на товары с истекающим в ближайшие 30 дней сроком.
    /// Исправлено: правильные имена столбцов (stock вместо quantity, purchase_price, UUID-типы).
    /// </summary>
    public static class ExpiryService
    {
        private const int DiscountThresholdDays = 30;
        private const decimal DiscountPercent = 0.20m;

        public static void ProcessBatches(string connStr)
        {
            using var conn = new NpgsqlConnection(connStr);
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                var warnings = new List<string>();

                // ── 1. Списание просроченных партий ──────────────────────────
                using (var cmd = new NpgsqlCommand(@"
                    SELECT sb.id, sb.product_id, sb.quantity, p.purchase_price, p.name
                    FROM stock_batches sb
                    JOIN products p ON p.id = sb.product_id
                    WHERE sb.expiry_date < CURRENT_DATE AND sb.quantity > 0", conn, tx))
                using (var reader = cmd.ExecuteReader())
                {
                    var expired = new List<(Guid batchId, Guid productId, int qty, decimal price, string name)>();
                    while (reader.Read())
                    {
                        expired.Add((
                            reader.GetGuid(0),    // sb.id — UUID
                            reader.GetGuid(1),    // sb.product_id — UUID
                            reader.GetInt32(2),   // sb.quantity
                            reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),  // p.purchase_price
                            reader.GetString(4)   // p.name
                        ));
                    }
                    reader.Close();

                    foreach (var b in expired)
                    {
                        // Добавляем запись о списании
                        using var cmdWriteOff = new NpgsqlCommand(@"
                            INSERT INTO write_offs (batch_id, quantity, reason)
                            VALUES (@bid, @qty, 'Срок годности истёк')", conn, tx);
                        cmdWriteOff.Parameters.AddWithValue("@bid", b.batchId);
                        cmdWriteOff.Parameters.AddWithValue("@qty", b.qty);
                        cmdWriteOff.ExecuteNonQuery();

                        // Обнуляем партию
                        using var cmdBatch = new NpgsqlCommand(
                            "UPDATE stock_batches SET quantity = 0 WHERE id = @id", conn, tx);
                        cmdBatch.Parameters.AddWithValue("@id", b.batchId);
                        cmdBatch.ExecuteNonQuery();

                        // Уменьшаем остаток товара (колонка stock, не quantity)
                        using var cmdProduct = new NpgsqlCommand(
                            "UPDATE products SET stock = GREATEST(0, stock - @qty) WHERE id = @pid", conn, tx);
                        cmdProduct.Parameters.AddWithValue("@qty", b.qty);
                        cmdProduct.Parameters.AddWithValue("@pid", b.productId);
                        cmdProduct.ExecuteNonQuery();

                        warnings.Add($"СПИСАНО: «{b.name}» — {b.qty} шт., убыток {b.qty * b.price:N2} ₽");
                    }
                }

                // ── 2. Скидка на товары с истекающим сроком ───────────────────
                using (var cmd = new NpgsqlCommand(@"
                    SELECT DISTINCT sb.product_id, p.name, p.purchase_price
                    FROM stock_batches sb
                    JOIN products p ON p.id = sb.product_id
                    WHERE sb.expiry_date BETWEEN CURRENT_DATE AND CURRENT_DATE + @days
                      AND sb.quantity > 0", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@days", DiscountThresholdDays);
                    using var reader = cmd.ExecuteReader();
                    var nearExpiry = new List<(Guid productId, string name, decimal price)>();
                    while (reader.Read())
                    {
                        nearExpiry.Add((
                            reader.GetGuid(0),
                            reader.GetString(1),
                            reader.IsDBNull(2) ? 0m : reader.GetDecimal(2)
                        ));
                    }
                    reader.Close();

                    foreach (var item in nearExpiry)
                    {
                        decimal discounted = Math.Round(item.price * (1 - DiscountPercent), 2);
                        // Применяем скидку только если текущая цена закупки выше скидочной
                        using var cmdDiscount = new NpgsqlCommand(@"
                            UPDATE products SET purchase_price = @p
                            WHERE id = @id AND purchase_price > @p", conn, tx);
                        cmdDiscount.Parameters.AddWithValue("@p", discounted);
                        cmdDiscount.Parameters.AddWithValue("@id", item.productId);
                        int affected = cmdDiscount.ExecuteNonQuery();
                        if (affected > 0)
                            warnings.Add($"СКИДКА 20%: «{item.name}» — новая цена {discounted:N2} ₽");
                    }
                }

                tx.Commit();

                if (warnings.Count > 0)
                {
                    var sb = new StringBuilder("⚠ Обработка сроков годности:\n\n");
                    foreach (var w in warnings) sb.AppendLine(w);
                    MessageBox.Show(sb.ToString(), "Сроки годности", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
