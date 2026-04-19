using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenStock.Models;

/// <summary>
/// Представляет товар на складе.
/// </summary>
[Table("products")]
public class Product
{
    /// <summary>
    /// Уникальный идентификатор товара (UUID).
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Артикул товара.
    /// </summary>
    [Column("article")]
    public string Article { get; set; } = string.Empty;

    /// <summary>
    /// Название товара.
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор категории (UUID).
    /// </summary>
    [Column("category_id")]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Единица измерения.
    /// </summary>
    [Column("unit")]
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Закупочная цена.
    /// </summary>
    [Column("purchase_price")]
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// Цена продажи (используется для расчёта прибыли в отчётах).
    /// </summary>
    [Column("selling_price")]
    public decimal SellingPrice { get; set; }

    /// <summary>
    /// Остаток на складе.
    /// </summary>
    [Column("stock")]
    public int Stock { get; set; }

    /// <summary>
    /// Срок годности товара. <c>null</c> означает бессрочный товар.
    /// </summary>
    [Column("expiry_date")]
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>
    /// Категория товара.
    /// </summary>
    public Category Category { get; set; } = null!;

    /// <summary>
    /// Коллекция позиций в отгрузках.
    /// </summary>
    public ICollection<ShipmentItem> ShipmentItems { get; set; } = new List<ShipmentItem>();
}
