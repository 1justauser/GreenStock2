using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenStock.Models;

/// <summary>
/// Представляет позицию в отгрузке.
/// </summary>
[Table("shipment_items")]
public class ShipmentItem
{
    /// <summary>
    /// Уникальный идентификатор позиции (UUID).
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор отгрузки (UUID).
    /// </summary>
    [Column("shipment_id")]
    public Guid ShipmentId { get; set; }

    /// <summary>
    /// Идентификатор товара (UUID).
    /// </summary>
    [Column("product_id")]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Количество товара.
    /// </summary>
    [Column("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// Отгрузка, к которой относится позиция.
    /// </summary>
    public Shipment Shipment { get; set; } = null!;

    /// <summary>
    /// Товар в позиции.
    /// </summary>
    public Product Product { get; set; } = null!;
}