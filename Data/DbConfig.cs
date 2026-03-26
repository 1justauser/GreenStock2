namespace GreenStock.Data;

/// <summary>
/// Хранит строку подключения к базе данных PostgreSQL.
/// Измените <see cref="ConnectionString"/> в соответствии с вашей конфигурацией.
/// </summary>
public static class DbConfig
{
    /// <summary>
    /// Строка подключения к PostgreSQL.
    /// </summary>
    public static string ConnectionString { get; set; } =
        "Host=localhost;Port=5432;Database=greenstock;Username=postgres;Password=postgres";

    /// <summary>
    /// Если <c>true</c>, контекст будет использовать InMemory-базу вместо PostgreSQL.
    /// Устанавливается в тестах перед каждым тестом.
    /// </summary>
    public static bool UseInMemory { get; set; } = false;

    /// <summary>
    /// Имя InMemory-базы для тестов. Меняется перед каждым тестом для полной изоляции.
    /// </summary>
    public static string InMemoryDbName { get; set; } = "GreenStockTestDb";
}
