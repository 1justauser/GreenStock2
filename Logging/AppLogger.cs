using NLog;

namespace GreenStock.Logging;

/// <summary>
/// Централизованная фабрика логгеров на базе NLog.
/// Используйте <see cref="For{T}"/> или <see cref="For(string)"/> для получения логгера.
/// </summary>
public static class AppLogger
{
    /// <summary>
    /// Возвращает логгер, привязанный к типу <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Тип, для которого создаётся логгер.</typeparam>
    public static ILogger For<T>() => LogManager.GetLogger(typeof(T).FullName);

    /// <summary>
    /// Возвращает логгер с произвольным именем.
    /// </summary>
    /// <param name="name">Имя логгера.</param>
    public static ILogger For(string name) => LogManager.GetLogger(name);

    /// <summary>
    /// Завершает работу NLog и сбрасывает буферы на диск.
    /// Вызывайте при завершении приложения.
    /// </summary>
    public static void Shutdown() => LogManager.Shutdown();
}
