using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenStock.Models;

/// <summary>
/// Представляет пользователя системы.
/// </summary>
[Table("users")]
public class User
{
    /// <summary>
    /// Уникальный идентификатор пользователя (UUID).
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Логин для входа в систему.
    /// </summary>
    [Column("login")]
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Хэш пароля пользователя (BCrypt).
    /// </summary>
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя в системе.
    /// </summary>
    [Column("role")]
    public UserRole Role { get; set; }

    /// <summary>
    /// Коллекция отправок, созданных пользователем.
    /// </summary>
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}