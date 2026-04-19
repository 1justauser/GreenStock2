using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Npgsql;

namespace GreenStock2
{
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

                // 1. Списание просроченных
                using (var cmd = new NpgsqlCommand(@"
                    SELECT sb.id, sb.product_id, sb.quantity, p.purchase_price, p.name
                    FROM stock_batches sb JOIN products p ON p.id = sb.product_id
                    WHERE sb.expiry_date < CURRENT_DATE AND sb.quantity > 0", conn, tx))
                using (var reader = cmd.ExecuteReader())
                {
                    var expired = new List<(int batchId, int productId, int qty, decimal price, string name)>();
                    while (reader.Read())
                        expired.Add((reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2),
                                     reader.IsDBNull(3) ? 0 : reader.GetDecimal(3), reader.GetString(4)));
                    reader.Close();

                    foreach (var b in expired)
                    {
                        new NpgsqlCommand(@"INSERT INTO write_offs (batch_id, product_id, quantity, loss_amount, reason)
                            VALUES (@bid, @pid, @qty, @loss, 'Срок годности истёк')", conn, tx)
                        { Parameters = { new("@bid", b.batchId), new("@pid", b.productId), new("@qty", b.qty), new("@loss", b.qty * b.price) } }
                        .ExecuteNonQuery();

                        new NpgsqlCommand("UPDATE stock_batches SET quantity = 0 WHERE id = @id", conn, tx)
                        { Parameters = { new("@id", b.batchId) } }
                        .ExecuteNonQuery();

                        new NpgsqlCommand("UPDATE products SET quantity = quantity - @qty WHERE id = @pid", conn, tx)
                        { Parameters = { new("@qty", b.qty), new("@pid", b.productId) } }
                        .ExecuteNonQuery();

                        warnings.Add($"СПИСАНО: «{b.name}» — {b.qty} шт., убыток {b.qty * b.price:N2} ₽");
                    }
                }

                // 2. Скидка на близкие к сроку
                using (var cmd = new NpgsqlCommand(@"
                    SELECT DISTINCT sb.product_id, p.name, p.price
                    FROM stock_batches sb JOIN products p ON p.id = sb.product_id
                    WHERE sb.expiry_date BETWEEN CURRENT_DATE AND CURRENT_DATE + @days AND sb.quantity > 0", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@days", DiscountThresholdDays);
                    using var reader = cmd.ExecuteReader();
                    var nearExpiry = new List<(int productId, string name, decimal price)>();
                    while (reader.Read()) nearExpiry.Add((reader.GetInt32(0), reader.GetString(1), reader.GetDecimal(2)));
                    reader.Close();

                    foreach (var item in nearExpiry)
                    {
                        decimal discounted = Math.Round(item.price * (1 - DiscountPercent), 2);
                        int affected = new NpgsqlCommand("UPDATE products SET price = @p WHERE id = @id AND price > @p", conn, tx)
                        { Parameters = { new("@p", discounted), new("@id", item.productId) } }
                        .ExecuteNonQuery();
                        if (affected > 0) warnings.Add($"СКИДКА 20%: «{item.name}» — {discounted:N2} ₽");
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
            catch { tx.Rollback(); throw; }
        }
    }
}