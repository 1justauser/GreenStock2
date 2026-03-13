using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GreenStock.Data;
using GreenStock.Models;

namespace GreenStock.Forms
{
    public class RegisterForm : Form
    {
        private Label   lblLogin, lblPassword, lblConfirm, lblRole;
        private TextBox txtLogin, txtPassword, txtConfirm, txtRole;
        private Button  btnRegister, btnBack;
        private Label   lblError;

        public string RegisteredLogin { get; private set; } = string.Empty;

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var screen = Screen.PrimaryScreen!.WorkingArea;
            int W = screen.Width / 2;
            int H = screen.Height / 2;

            this.Text            = "Регистрация кладовщика";
            this.ClientSize      = new Size(W, H);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = Color.White;

            int fontSize = W / 60;
            int labelX   = W / 6;
            int inputX   = W / 6 + W / 8 + 75;
            int inputW   = W / 2;
            int fieldH   = H / 14;
            int rowH     = H / 9;
            int startY   = H / 12;

            Label MakeLabel(string text, int row) => new Label
            {
                Text     = text,
                Font     = new Font("Segoe UI", fontSize),
                Location = new Point(labelX, startY + row * rowH),
                AutoSize = true
            };

            TextBox MakeInput(int row, bool password = false) => new TextBox
            {
                Font         = new Font("Segoe UI", fontSize),
                Location     = new Point(inputX, startY + row * rowH - 2),
                Size         = new Size(inputW, fieldH),
                BorderStyle  = BorderStyle.FixedSingle,
                PasswordChar = password ? '*' : '\0'
            };

            lblLogin    = MakeLabel("Логин:", 0);
            txtLogin    = MakeInput(0);
            lblPassword = MakeLabel("Пароль:", 1);
            txtPassword = MakeInput(1, true);
            lblConfirm  = MakeLabel("Подтвердите:", 2);
            txtConfirm  = MakeInput(2, true);
            lblRole     = MakeLabel("Роль:", 3);
            txtRole     = new TextBox { Font = new Font("Segoe UI", fontSize), Location = new Point(inputX, startY + 3 * rowH - 2), Size = new Size(inputW, fieldH), BorderStyle = BorderStyle.FixedSingle, Text = "Kladovshik", ReadOnly = true, BackColor = Color.LightGray };

            var separator = new Panel { Location = new Point(labelX, startY + 4 * rowH - 5), Size = new Size(W - labelX * 2, 1), BackColor = Color.Silver };

            int btnY = startY + 4 * rowH + 10;
            btnRegister = new Button { Text = "Зарегистрироваться", Font = new Font("Segoe UI", fontSize, FontStyle.Bold), Size = new Size(W / 3, H / 9), BackColor = Color.FromArgb(28, 42, 74), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click  += BtnRegister_Click;
            this.AcceptButton   = btnRegister;
            this.Load += (s, e) => btnRegister.Location = new Point((W / 2) - btnRegister.Width - 10, btnY);

            btnBack = new Button { Text = "назад", Font = new Font("Segoe UI", fontSize), Size = new Size(W / 6, H / 9), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnBack.Click     += (s, e) => this.Close();
            this.CancelButton  = btnBack;
            this.Load += (s, e) => btnBack.Location = new Point((W / 2) + 10, btnY);

            lblError = new Label { Text = "", ForeColor = Color.Red, Font = new Font("Segoe UI", fontSize - 1), AutoSize = true, Location = new Point(0, btnY + H / 9 + 10) };
            this.Load += (s, e) => lblError.Left = (W - lblError.Width) / 2;

            this.Controls.AddRange(new Control[] { lblLogin, txtLogin, lblPassword, txtPassword, lblConfirm, txtConfirm, lblRole, txtRole, separator, btnRegister, btnBack, lblError });
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            lblError.Text = "";
            string login    = txtLogin.Text.Trim();
            string password = txtPassword.Text;
            string confirm  = txtConfirm.Text;

            if (string.IsNullOrEmpty(login))    { lblError.Text = "Введите логин"; return; }
            if (string.IsNullOrEmpty(password)) { lblError.Text = "Введите пароль"; return; }
            if (password != confirm)            { lblError.Text = "Пароли не совпадают"; txtConfirm.Clear(); txtConfirm.Focus(); return; }
            if (password.Length < 4)            { lblError.Text = "Пароль минимум 4 символа"; return; }

            try
            {
                using var db = new AppDbContext();
                if (db.Users.Any(u => u.Login == login)) { lblError.Text = "Логин уже занят"; return; }
                db.Users.Add(new User { Login = login, PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), Role = "Kladovshik" });
                db.SaveChanges();
                RegisteredLogin = login;
                MessageBox.Show($"Регистрация успешна!\nВойдите под логином «{login}».", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex) { lblError.Text = $"Ошибка: {ex.Message}"; }
        }
    }
}
