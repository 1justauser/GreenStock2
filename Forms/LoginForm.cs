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
        private Label lblTitle;
        private Label lblLogin, lblPassword;
        private TextBox txtLogin, txtPassword;
        private Button btnLogin;
        private Label lblError;
        private LinkLabel lnkRegister;

        public User? LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var screen = Screen.PrimaryScreen!.WorkingArea;
            int W = screen.Width / 2;
            int H = screen.Height / 2;

            this.Text = "Складской учет - Авторизация";
            this.ClientSize = new Size(W, H);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            lblTitle = new Label
            {
                Text = "GreenStock",
                Font = new Font("Segoe UI", W / 30, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 42, 74),
                AutoSize = true,
                Location = new Point(0, H / 8)
            };
            this.Load += (s, e) =>
                lblTitle.Left = (W - lblTitle.Width) / 2;

            int labelX = W / 6;
            int inputX = W / 6 + W / 8;
            int inputW = W / 2;
            int fieldH = H / 14;
            int row1Y = H * 35 / 100;
            int row2Y = H * 50 / 100;
            int fontSize = W / 60;

            lblLogin = new Label
            {
                Text = "Логин:",
                Font = new Font("Segoe UI", fontSize),
                Location = new Point(labelX, row1Y),
                AutoSize = true
            };
            txtLogin = new TextBox
            {
                Font = new Font("Segoe UI", fontSize),
                Location = new Point(inputX + 75, row1Y - 2),
                Size = new Size(inputW, fieldH),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblPassword = new Label
            {
                Text = "Пароль:",
                Font = new Font("Segoe UI", fontSize),
                Location = new Point(labelX, row2Y),
                AutoSize = true
            };
            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", fontSize),
                Location = new Point(inputX + 75, row2Y - 2),
                Size = new Size(inputW, fieldH),
                PasswordChar = '*',
                BorderStyle = BorderStyle.FixedSingle
            };

            btnLogin = new Button
            {
                Text = "ВОЙТИ",
                Font = new Font("Segoe UI", fontSize, FontStyle.Bold),
                Size = new Size(W / 4, H / 9),
                BackColor = Color.FromArgb(28, 42, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            this.AcceptButton = btnLogin;
            this.Load += (s, e) =>
                btnLogin.Location = new Point(
                    (W - btnLogin.Width) / 2, H * 65 / 100);

            lblError = new Label
            {
                Text = "Неверный логин или пароль",
                Font = new Font("Segoe UI", fontSize - 1),
                ForeColor = Color.Red,
                AutoSize = true,
                Location = new Point(0, H * 78 / 100),
                Visible = false
            };
            this.Load += (s, e) =>
                lblError.Left = (W - lblError.Width) / 2;

            lnkRegister = new LinkLabel
            {
                Text = "Зарегистрироваться",
                Font = new Font("Segoe UI", fontSize - 1),
                AutoSize = true,
                Location = new Point(0, H * 88 / 100),
                LinkColor = Color.FromArgb(40, 100, 200)
            };
            this.Load += (s, e) =>
                lnkRegister.Left = (W - lnkRegister.Width) / 2;
            lnkRegister.LinkClicked += (s, e) =>
            {
                var registerForm = new RegisterForm();
                registerForm.ShowDialog();
                if (!string.IsNullOrEmpty(registerForm.RegisteredLogin))
                    txtLogin.Text = registerForm.RegisteredLogin;
            };

            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblLogin, txtLogin,
                lblPassword, txtPassword,
                btnLogin, lblError, lnkRegister
            });
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            lblError.Visible = false;
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Введите логин и пароль";
                lblError.Visible = true;
                return;
            }

            try
            {
                using var db = new AppDbContext();
                var user = db.Users.FirstOrDefault(u => u.Login == login);

                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    lblError.Visible = true;
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                LoggedInUser = user;
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