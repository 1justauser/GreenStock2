using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenStock.Models;

/// <summary>
/// Представляет отгрузку товаров.
/// </summary>
[Table("shipments")]
public class Shipment
{
    /// <summary>
    /// Уникальный идентификатор отгрузки (UUID).
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор создателя (UUID).
    /// </summary>
    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Дата и время создания отгрузки.
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Получатель отгрузки.
    /// </summary>
    [Column("recipient")]
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Пользователь, создавший отгрузку.
    /// </summary>
    public User CreatedByUser { get; set; } = null!;

    /// <summary>
    /// Коллекция позиций отгрузки.
    /// </summary>
    public ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
}
