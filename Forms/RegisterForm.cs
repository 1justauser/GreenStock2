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
        private Label   lblLoginError, lblConfirmError;
        private Button  btnRegister, btnBack;

        public string RegisteredLogin { get; private set; } = string.Empty;

        public RegisterForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "Регистрация кладовщика";
            this.Size            = new Size(430, 380);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = Color.FromArgb(240, 240, 245);

            int labelX = 60;
            int inputX = 210;
            int inputW = 165;

            // ── Логин ─────────────────────────────────────────
            lblLogin = new Label { Text = "Логин:", Font = new Font("Segoe UI", 11), Location = new Point(labelX, 30), AutoSize = true };
            txtLogin = new TextBox { Font = new Font("Segoe UI", 11), Location = new Point(inputX, 27), Size = new Size(inputW, 26) };
            lblLoginError = new Label { Text = "Логин уже занят", Font = new Font("Segoe UI", 8), ForeColor = Color.Red, Location = new Point(inputX, 56), AutoSize = true, Visible = false };

            // ── Пароль ────────────────────────────────────────
            lblPassword = new Label { Text = "Пароль:", Font = new Font("Segoe UI", 11), Location = new Point(labelX, 80), AutoSize = true };
            txtPassword = new TextBox { Font = new Font("Segoe UI", 11), Location = new Point(inputX, 77), Size = new Size(inputW, 26), PasswordChar = '*' };

            // ── Подтвердите пароль ────────────────────────────
            lblConfirm = new Label { Text = "Подтвердите пароль:", Font = new Font("Segoe UI", 11), Location = new Point(labelX, 130), AutoSize = true };
            txtConfirm = new TextBox { Font = new Font("Segoe UI", 11), Location = new Point(inputX, 127), Size = new Size(inputW, 26), PasswordChar = '*' };
            lblConfirmError = new Label { Text = "Пароли не совпадают", Font = new Font("Segoe UI", 8), ForeColor = Color.Red, Location = new Point(inputX, 156), AutoSize = true, Visible = false };

            // ── Роль ──────────────────────────────────────────
            lblRole = new Label { Text = "Роль:", Font = new Font("Segoe UI", 11), Location = new Point(labelX, 185), AutoSize = true };
            txtRole = new TextBox { Font = new Font("Segoe UI", 11), Location = new Point(inputX, 182), Size = new Size(inputW, 26), Text = "Кладовщик", ReadOnly = true, BackColor = Color.FromArgb(220, 220, 220) };

            // ── Separator ─────────────────────────────────────
            var sep = new Panel { Location = new Point(20, 230), Size = new Size(375, 1), BackColor = Color.Silver };

            // ── Buttons ───────────────────────────────────────
            btnRegister = new Button
            {
                Text      = "Зарегистрироваться",
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Location  = new Point(90, 255),
                Size      = new Size(175, 34),
                BackColor = Color.FromArgb(28, 42, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click  += BtnRegister_Click;
            this.AcceptButton   = btnRegister;

            btnBack = new Button
            {
                Text      = "Назад",
                Font      = new Font("Segoe UI", 10),
                Location  = new Point(275, 255),
                Size      = new Size(90, 34),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnBack.Click     += (s, e) => this.Close();
            this.CancelButton  = btnBack;

            this.Controls.AddRange(new Control[]
            {
                lblLogin, txtLogin, lblLoginError,
                lblPassword, txtPassword,
                lblConfirm, txtConfirm, lblConfirmError,
                lblRole, txtRole,
                sep, btnRegister, btnBack
            });
        }

        private void SetFieldError(TextBox txt, bool hasError)
        {
            txt.BackColor = hasError ? Color.FromArgb(255, 220, 220) : Color.White;
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            // Reset errors
            lblLoginError.Visible   = false;
            lblConfirmError.Visible = false;
            SetFieldError(txtLogin, false);
            SetFieldError(txtConfirm, false);

            string login    = txtLogin.Text.Trim();
            string password = txtPassword.Text;
            string confirm  = txtConfirm.Text;

            bool valid = true;

            if (string.IsNullOrEmpty(login)) { SetFieldError(txtLogin, true); valid = false; }
            if (string.IsNullOrEmpty(password)) { SetFieldError(txtPassword, true); valid = false; }
            if (password != confirm)
            {
                lblConfirmError.Text    = "Пароли не совпадают";
                lblConfirmError.Visible = true;
                SetFieldError(txtConfirm, true);
                valid = false;
            }
            if (!valid) return;

            try
            {
                using var db = new AppDbContext();
                if (db.Users.Any(u => u.Login == login))
                {
                    lblLoginError.Text    = "Логин уже занят";
                    lblLoginError.Visible = true;
                    SetFieldError(txtLogin, true);
                    return;
                }

                db.Users.Add(new User
                {
                    Login        = login,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role         = "Kladovshik"
                    
                });
                db.SaveChanges();

                RegisteredLogin = login;
                MessageBox.Show($"Регистрация успешна!\nВойдите под логином «{login}».",
                    "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
