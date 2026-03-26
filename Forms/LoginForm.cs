using GreenStock.Data;
using System.ComponentModel;
using GreenStock.Logging;
using GreenStock.Models;
using GreenStock.Resources;
using NLog;

namespace GreenStock.Forms;

/// <summary>
/// Форма авторизации пользователя.
/// </summary>
public class LoginForm : Form
{
    private static readonly ILogger _log = AppLogger.For<LoginForm>();

    private Label     _lblTitle    = null!;
    private Label     _lblLogin    = null!;
    private Label     _lblPassword = null!;
    private TextBox   _txtLogin    = null!;
    private TextBox   _txtPassword = null!;
    private Label     _lblError    = null!;
    private Button    _btnLogin    = null!;
    private LinkLabel _lnkRegister = null!;

    /// <summary>
    /// Пользователь, успешно прошедший авторизацию.
    /// <c>null</c>, если вход не был выполнен.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public User? LoggedInUser { get; private set; }

    /// <summary>
    /// Инициализирует новый экземпляр формы авторизации.
    /// </summary>
    public LoginForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text            = Strings.Login_Title;
        Size            = new Size(430, 360);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = Color.FromArgb(240, 240, 245);

        _lblTitle = new Label
        {
            Text      = Strings.Login_AppTitle,
            Font      = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(28, 42, 74),
            AutoSize  = true,
            Location  = new Point(0, 30)
        };
        Load += (s, e) => _lblTitle.Left = (ClientSize.Width - _lblTitle.Width) / 2;

        _lblLogin = new Label
        {
            Text     = Strings.Login_LabelLogin,
            Font     = new Font("Segoe UI", 11),
            Location = new Point(70, 100),
            AutoSize = true
        };
        _txtLogin = new TextBox
        {
            Font     = new Font("Segoe UI", 11),
            Location = new Point(155, 97),
            Size     = new Size(185, 26)
        };

        _lblPassword = new Label
        {
            Text     = Strings.Login_LabelPassword,
            Font     = new Font("Segoe UI", 11),
            Location = new Point(70, 145),
            AutoSize = true
        };
        _txtPassword = new TextBox
        {
            Font         = new Font("Segoe UI", 11),
            Location     = new Point(155, 142),
            Size         = new Size(185, 26),
            PasswordChar = '*'
        };

        _lblError = new Label
        {
            Text      = Strings.Login_ErrInvalidCredentials,
            Font      = new Font("Segoe UI", 9),
            ForeColor = Color.Red,
            AutoSize  = true,
            Location  = new Point(0, 185),
            Visible   = false
        };
        Load += (s, e) => _lblError.Left = (ClientSize.Width - _lblError.Width) / 2;

        _btnLogin = new Button
        {
            Text      = Strings.Login_BtnLogin,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Size      = new Size(110, 32),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(28, 42, 74),
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnLogin.FlatAppearance.BorderColor = Color.FromArgb(28, 42, 74);
        _btnLogin.FlatAppearance.BorderSize  = 1;
        _btnLogin.Click  += BtnLogin_Click;
        AcceptButton      = _btnLogin;
        Load += (s, e) => _btnLogin.Location =
            new Point((ClientSize.Width - _btnLogin.Width) / 2, 210);

        _lnkRegister = new LinkLabel
        {
            Text      = Strings.Login_LinkRegister,
            Font      = new Font("Segoe UI", 10),
            AutoSize  = true,
            Location  = new Point(0, 260),
            LinkColor = Color.FromArgb(40, 100, 200)
        };
        Load += (s, e) => _lnkRegister.Left = (ClientSize.Width - _lnkRegister.Width) / 2;
        _lnkRegister.LinkClicked += (s, e) =>
        {
            var reg = new RegisterForm();
            reg.ShowDialog();
            if (!string.IsNullOrEmpty(reg.RegisteredLogin))
                _txtLogin.Text = reg.RegisteredLogin;
        };

        Controls.AddRange(new Control[]
        {
            _lblTitle,
            _lblLogin, _txtLogin,
            _lblPassword, _txtPassword,
            _lblError, _btnLogin,
            _lnkRegister
        });
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        _lblError.Visible = false;
        var login    = _txtLogin.Text.Trim();
        var password = _txtPassword.Text;

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            _lblError.Text    = Strings.Login_ErrEmptyFields;
            _lblError.Visible = true;
            return;
        }

        try
        {
            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u => u.Login == login);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _log.Warn("Неудачная попытка входа: логин «{0}»", login);
                _lblError.Text    = Strings.Login_ErrInvalidCredentials;
                _lblError.Visible = true;
                _txtPassword.Clear();
                _txtPassword.Focus();
                return;
            }

            _log.Info("Успешный вход: {0} ({1})", user.Login, user.Role);
            LoggedInUser  = user;
            DialogResult  = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Ошибка подключения к БД при входе");
            MessageBox.Show($"{Strings.Login_ErrDbConnection}\n{ex.Message}",
                Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
