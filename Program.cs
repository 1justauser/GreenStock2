using GreenStock.Data;
using GreenStock.Forms;
using GreenStock.Logging;

namespace GreenStock;

/// <summary>
/// Точка входа в приложение GreenStock.
/// </summary>
internal static class Program
{
    private static readonly NLog.ILogger _log = AppLogger.For("GreenStock.Program");

    /// <summary>
    /// Главный метод приложения.
    /// Запускает цикл: Авторизация → Каталог → снова Авторизация (при выходе).
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        _log.Info("Приложение GreenStock запущено");

        try
        {
            while (true)
            {
                var loginForm = new LoginForm();
                if (loginForm.ShowDialog() != DialogResult.OK || loginForm.LoggedInUser == null)
                {
                    _log.Info("Пользователь закрыл форму входа — выход из приложения");
                    break;
                }

                _log.Info("Пользователь {0} ({1}) выполнил вход",
                    loginForm.LoggedInUser.Login,
                    loginForm.LoggedInUser.Role);

                var catalogForm = new CatalogForm(loginForm.LoggedInUser);
                catalogForm.ShowDialog();

                _log.Info("Пользователь {0} вышел из каталога", loginForm.LoggedInUser.Login);
            }
        }
        catch (Exception ex)
        {
            _log.Fatal(ex, "Необработанное исключение на уровне приложения");
            MessageBox.Show(
                $"Критическая ошибка:\n{ex.Message}",
                "GreenStock",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _log.Info("Приложение GreenStock завершает работу");
            AppLogger.Shutdown();
        }
    }
}
