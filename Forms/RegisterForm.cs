using GreenStock.Data;
using System.ComponentModel;
using GreenStock.Logging;
using GreenStock.Models;
using GreenStock.Resources;
using NLog;

namespace GreenStock.Forms;

/// <summary>
/// Форма регистрации нового кладовщика.
/// </summary>
public class RegisterForm : Form
{
    private static readonly ILogger _log = AppLogger.For<RegisterForm>();

    private Label   _lblLogin        = null!;
    private Label   _lblPassword     = null!;
    private Label   _lblConfirm      = null!;
    private Label   _lblRole         = null!;
    private TextBox _txtLogin        = null!;
    private TextBox _txtPassword     = null!;
    private TextBox _txtConfirm      = null!;
    private TextBox _txtRole         = null!;
    private Label   _lblLoginError   = null!;
    private Label   _lblConfirmError = null!;
    private Button  _btnRegister     = null!;
    private Button  _btnBack         = null!;

    /// <summary>
    /// Логин успешно зарегистрированного пользователя.
    /// Пустая строка, если регистрация не была завершена.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string RegisteredLogin { get; private set; } = string.Empty;

    /// <summary>
    /// Инициализирует новый экземпляр формы регистрации.
    /// </summary>
    public RegisterForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text            = Strings.Register_Title;
        Size            = new Size(430, 380);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = Color.FromArgb(240, 240, 245);

        const int labelX = 60;
        const int inputX = 210;
        const int inputW = 165;

        _lblLogin = new Label
            { Text = Strings.Register_LabelLogin, Font = new Font("Segoe UI", 11), Location = new Point(labelX, 30), AutoSize = true };
        _txtLogin = new TextBox
            { Font = new Font("Segoe UI", 11), Location = new Point(inputX, 27), Size = new Size(inputW, 26) };
        _lblLoginError = new Label
            { Text = Strings.Register_ErrLoginTaken, Font = new Font("Segoe UI", 8), ForeColor = Color.Red, Location = new Point(inputX, 56), AutoSize = true, Visible = false };

        _lblPassword = new Label
            { Text = Strings.Register_LabelPassword, Font = new Font("Segoe UI", 11), Location = new Point(labelX, 80), AutoSize = true };
        _txtPassword = new TextBox
            { Font = new Font("Segoe UI", 11), Location = new Point(inputX, 77), Size = new Size(inputW, 26), PasswordChar = '*' };

        _lblConfirm = new Label
            { Text = Strings.Register_LabelConfirm, Font = new Font("Segoe UI", 11), Location = new Point(labelX, 130), AutoSize = true };
        _txtConfirm = new TextBox
            { Font = new Font("Segoe UI", 11), Location = new Point(inputX, 127), Size = new Size(inputW, 26), PasswordChar = '*' };
        _lblConfirmError = new Label
            { Text = Strings.Register_ErrPasswordMismatch, Font = new Font("Segoe UI", 8), ForeColor = Color.Red, Location = new Point(inputX, 156), AutoSize = true, Visible = false };

        _lblRole = new Label
            { Text = Strings.Register_LabelRole, Font = new Font("Segoe UI", 11), Location = new Point(labelX, 185), AutoSize = true };
        _txtRole = new TextBox
        {
            Font      = new Font("Segoe UI", 11),
            Location  = new Point(inputX, 182),
            Size      = new Size(inputW, 26),
            Text      = Strings.Register_RoleKladovshik,
            ReadOnly  = true,
            BackColor = Color.FromArgb(220, 220, 220)
        };

        var sep = new Panel
            { Location = new Point(20, 230), Size = new Size(375, 1), BackColor = Color.Silver };

        _btnRegister = new Button
        {
            Text      = Strings.Register_BtnRegister,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Location  = new Point(90, 255),
            Size      = new Size(175, 34),
            BackColor = Color.FromArgb(28, 42, 74),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnRegister.FlatAppearance.BorderSize = 0;
        _btnRegister.Click += BtnRegister_Click;
        AcceptButton        = _btnRegister;

        _btnBack = new Button
        {
            Text      = Strings.Get("Back"),
            Font      = new Font("Segoe UI", 10),
            Location  = new Point(275, 255),
            Size      = new Size(90, 34),
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnBack.Click += (s, e) => Close();
        CancelButton    = _btnBack;

        Controls.AddRange(new Control[]
        {
            _lblLogin, _txtLogin, _lblLoginError,
            _lblPassword, _txtPassword,
            _lblConfirm, _txtConfirm, _lblConfirmError,
            _lblRole, _txtRole,
            sep, _btnRegister, _btnBack
        });
    }

    private void SetFieldError(TextBox txt, bool hasError)
    {
        txt.BackColor = hasError ? Color.FromArgb(255, 220, 220) : Color.White;
    }

    private void BtnRegister_Click(object? sender, EventArgs e)
    {
        _lblLoginError.Visible   = false;
        _lblConfirmError.Visible = false;
        SetFieldError(_txtLogin, false);
        SetFieldError(_txtConfirm, false);

        var login    = _txtLogin.Text.Trim();
        var password = _txtPassword.Text;
        var confirm  = _txtConfirm.Text;
        var valid    = true;

        if (string.IsNullOrEmpty(login))    { SetFieldError(_txtLogin, true);    valid = false; }
        if (string.IsNullOrEmpty(password)) { SetFieldError(_txtPassword, true); valid = false; }
        if (password != confirm)
        {
            _lblConfirmError.Text    = Strings.Register_ErrPasswordMismatch;
            _lblConfirmError.Visible = true;
            SetFieldError(_txtConfirm, true);
            valid = false;
        }
        if (!valid) return;

        try
        {
            using var db = new AppDbContext();
            if (db.Users.Any(u => u.Login == login))
            {
                _log.Warn("Попытка регистрации с занятым логином: {0}", login);
                _lblLoginError.Text    = Strings.Register_ErrLoginTaken;
                _lblLoginError.Visible = true;
                SetFieldError(_txtLogin, true);
                return;
            }

            // Роль — всегда Kladovshik: регистрация через форму доступна только кладовщикам
            db.Users.Add(new User
            {
                Login        = login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role         = UserRole.Kladovshik
            });
            db.SaveChanges();

            _log.Info("Зарегистрирован новый пользователь: {0} (Kladovshik)", login);
            RegisteredLogin = login;
            MessageBox.Show(
                Strings.Register_SuccessMsg(login),
                Strings.Done,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Ошибка при регистрации пользователя {0}", login);
            MessageBox.Show($"{Strings.Error}: {ex.Message}",
                Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
