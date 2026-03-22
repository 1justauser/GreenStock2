using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GreenStock.Data;
using GreenStock.Models;

namespace GreenStock.Forms
{
    public class LoginForm : Form
    {
        private Label     lblTitle;
        private Label     lblLogin, lblPassword;
        private TextBox   txtLogin, txtPassword;
        private Label     lblError;
        private Button    btnLogin;
        private LinkLabel lnkRegister;

        public User? LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "Складской учет - Авторизация";
            this.Size            = new Size(430, 360);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = Color.FromArgb(240, 240, 245);

            // ── ГринСток ──────────────────────────────────────
            lblTitle = new Label
            {
                Text      = "🌱 ГринСток",
                Font      = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 42, 74),
                AutoSize  = true,
                Location  = new Point(0, 30)
            };
            this.Load += (s, e) =>
                lblTitle.Left = (this.ClientSize.Width - lblTitle.Width) / 2;

            // ── Логин ─────────────────────────────────────────
            lblLogin = new Label
            {
                Text     = "Логин:",
                Font     = new Font("Segoe UI", 11),
                Location = new Point(70, 100),
                AutoSize = true
            };
            txtLogin = new TextBox
            {
                Font     = new Font("Segoe UI", 11),
                Location = new Point(155, 97),
                Size     = new Size(185, 26)
            };

            // ── Пароль ────────────────────────────────────────
            lblPassword = new Label
            {
                Text     = "Пароль:",
                Font     = new Font("Segoe UI", 11),
                Location = new Point(70, 145),
                AutoSize = true
            };
            txtPassword = new TextBox
            {
                Font         = new Font("Segoe UI", 11),
                Location     = new Point(155, 142),
                Size         = new Size(185, 26),
                PasswordChar = '*'
            };

            // ── Error (above button) ──────────────────────────
            lblError = new Label
            {
                Text      = "Неверный логин или пароль",
                Font      = new Font("Segoe UI", 9),
                ForeColor = Color.Red,
                AutoSize  = true,
                Location  = new Point(0, 185),
                Visible   = false
            };
            this.Load += (s, e) =>
                lblError.Left = (this.ClientSize.Width - lblError.Width) / 2;

            // ── ВОЙТИ ─────────────────────────────────────────
            btnLogin = new Button
            {
                Text      = "ВОЙТИ",
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Size      = new Size(110, 32),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(28, 42, 74),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderColor = Color.FromArgb(28, 42, 74);
            btnLogin.FlatAppearance.BorderSize  = 1;
            btnLogin.Click  += BtnLogin_Click;
            this.AcceptButton = btnLogin;
            this.Load += (s, e) =>
                btnLogin.Location = new Point(
                    (this.ClientSize.Width - btnLogin.Width) / 2, 210);

            // ── Зарегистрироваться ────────────────────────────
            lnkRegister = new LinkLabel
            {
                Text      = "Зарегистрироваться",
                Font      = new Font("Segoe UI", 10),
                AutoSize  = true,
                Location  = new Point(0, 260),
                LinkColor = Color.FromArgb(40, 100, 200)
            };
            this.Load += (s, e) =>
                lnkRegister.Left = (this.ClientSize.Width - lnkRegister.Width) / 2;
            lnkRegister.LinkClicked += (s, e) =>
            {
                var reg = new RegisterForm();
                reg.ShowDialog();
                if (!string.IsNullOrEmpty(reg.RegisteredLogin))
                    txtLogin.Text = reg.RegisteredLogin;
            };

            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblLogin, txtLogin,
                lblPassword, txtPassword,
                lblError, btnLogin,
                lnkRegister
            });
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            lblError.Visible = false;
            string login    = txtLogin.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                lblError.Text    = "Введите логин и пароль";
                lblError.Visible = true;
                return;
            }

            try
            {
                using var db = new AppDbContext();
                var user     = db.Users.FirstOrDefault(u => u.Login == login);

                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    lblError.Visible = true;
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                LoggedInUser      = user;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
