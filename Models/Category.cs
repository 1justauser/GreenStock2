using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenStock.Models;

/// <summary>
/// Представляет категорию товаров.
/// </summary>
[Table("categories")]
public class Category
{
    /// <summary>
    /// Уникальный идентификатор категории (UUID).
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Название категории.
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Коллекция товаров в категории.
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}