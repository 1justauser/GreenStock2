namespace GreenStock.Models;

/// <summary>
/// Роли пользователей в системе складского учёта.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Администратор с полными правами управления.
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Кладовщик с правами на работу с отгрузками.
    /// </summary>
    Kladovshik = 2
}