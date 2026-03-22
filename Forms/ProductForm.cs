using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GreenStock.Data;
using GreenStock.Models;

namespace GreenStock.Forms
{
    public class ProductForm : Form
    {
        private readonly Product? _existing;

        private Label         lblArticle, lblName, lblCategory, lblUnit, lblPrice, lblStock;
        private TextBox       txtArticle, txtName;
        private TextBox       txtStock; // ReadOnly
        private ComboBox      cmbCategory, cmbUnit;
        private NumericUpDown nudPrice;
        private Label         lblArticleError, lblNameError;
        private Button        btnSave, btnCancel;
        private Label         lblRequired;

        public ProductForm(Product? existing)
        {
            _existing = existing;
            InitializeComponent();
            LoadCategories();
            if (_existing != null) FillFields();
        }

        private void InitializeComponent()
        {
            bool isEdit = _existing != null;

            this.Text            = isEdit ? "Редактировать товар" : "Добавить товар";
            this.Size            = new Size(400, 420);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.FromArgb(240, 240, 245);

            int labelX = 30;
            int inputX = 165;
            int inputW = 185;
            int rowH   = 50;
            int startY = 20;

            Label MakeLabel(string text, int row) => new Label
            {
                Text     = text,
                Font     = new Font("Segoe UI", 10),
                Location = new Point(labelX, startY + row * rowH + 4),
                AutoSize = true
            };

            // ── Артикул ───────────────────────────────────────
            lblArticle  = MakeLabel("Артикул*:", 0);
            txtArticle  = new TextBox
            {
                Font      = new Font("Segoe UI", 10),
                Location  = new Point(inputX, startY + 0 * rowH),
                Size      = new Size(inputW, 24),
                ReadOnly  = isEdit,
                BackColor = isEdit ? Color.FromArgb(220, 220, 220) : Color.White
            };
            lblArticleError = new Label { Text = "Артикул уже существует", Font = new Font("Segoe UI", 8), ForeColor = Color.Red, Location = new Point(inputX, startY + 0 * rowH + 26), AutoSize = true, Visible = false };

            // ── Название ──────────────────────────────────────
            lblName  = MakeLabel("Название*:", 1);
            txtName  = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(inputX, startY + 1 * rowH), Size = new Size(inputW, 24) };
            lblNameError = new Label { Text = "Поле обязательно для заполнения", Font = new Font("Segoe UI", 8), ForeColor = Color.Red, Location = new Point(inputX, startY + 1 * rowH + 26), AutoSize = true, Visible = false };

            // ── Категория ─────────────────────────────────────
            lblCategory = MakeLabel("Категория*:", 2);
            cmbCategory = new ComboBox { Font = new Font("Segoe UI", 10), Location = new Point(inputX, startY + 2 * rowH), Size = new Size(inputW, 24), DropDownStyle = ComboBoxStyle.DropDownList };

            // ── Единица изм. ──────────────────────────────────
            lblUnit = MakeLabel("Единица изм.:", 3);
            cmbUnit = new ComboBox { Font = new Font("Segoe UI", 10), Location = new Point(inputX, startY + 3 * rowH), Size = new Size(100, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUnit.Items.AddRange(new[] { "шт", "пак", "кг", "л", "г" });
            cmbUnit.SelectedIndex = 0;

            // ── Цена закупки ──────────────────────────────────
            lblPrice = MakeLabel("Цена закупки:", 4);
            nudPrice = new NumericUpDown { Font = new Font("Segoe UI", 10), Location = new Point(inputX, startY + 4 * rowH), Size = new Size(80, 24), Minimum = 0, Maximum = 999999, DecimalPlaces = 2 };
            var lblRub = new Label { Text = "руб.", Font = new Font("Segoe UI", 10), Location = new Point(inputX + 86, startY + 4 * rowH + 4), AutoSize = true };

            // ── Текущий остаток (ReadOnly) ─────────────────────
            lblStock = MakeLabel("Текущий остаток:", 5);
            txtStock = new TextBox
            {
                Font      = new Font("Segoe UI", 10),
                Location  = new Point(inputX, startY + 5 * rowH),
                Size      = new Size(60, 24),
                Text      = isEdit ? _existing!.Stock.ToString() : "0",
                ReadOnly  = true,
                BackColor = Color.FromArgb(220, 220, 220)
            };
            var lblSht = new Label { Text = "шт.", Font = new Font("Segoe UI", 10), Location = new Point(inputX + 66, startY + 5 * rowH + 4), AutoSize = true };

            // ── Required hint ─────────────────────────────────
            lblRequired = new Label { Text = "*- обязательное поле", Font = new Font("Segoe UI", 8), ForeColor = Color.Gray, Location = new Point(labelX, startY + 6 * rowH + 5), AutoSize = true };

            // ── Buttons ───────────────────────────────────────
            int btnY = startY + 6 * rowH + 28;
            btnSave = new Button
            {
                Text      = "Сохранить",
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Location  = new Point(160, btnY),
                Size      = new Size(100, 32),
                BackColor = Color.FromArgb(28, 42, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click    += BtnSave_Click;
            this.AcceptButton = btnSave;

            btnCancel = new Button
            {
                Text      = "Отмена",
                Font      = new Font("Segoe UI", 10),
                Location  = new Point(270, btnY),
                Size      = new Size(90, 32),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnCancel.Click    += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.CancelButton   = btnCancel;

            this.Controls.AddRange(new Control[]
            {
                lblArticle, txtArticle, lblArticleError,
                lblName, txtName, lblNameError,
                lblCategory, cmbCategory,
                lblUnit, cmbUnit,
                lblPrice, nudPrice, lblRub,
                lblStock, txtStock, lblSht,
                lblRequired, btnSave, btnCancel
            });
        }

        private void LoadCategories()
        {
            using var db = new AppDbContext();
            var cats = db.Categories.OrderBy(c => c.Name).ToList();
            cmbCategory.Items.Clear();
            foreach (var c in cats) cmbCategory.Items.Add(c.Name);
            if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;
        }

        private void FillFields()
        {
            txtArticle.Text  = _existing!.Article;
            txtName.Text     = _existing.Name;
            cmbCategory.Text = _existing.Category?.Name ?? "";
            cmbUnit.Text     = _existing.Unit;
            nudPrice.Value   = _existing.PurchasePrice;
            txtStock.Text    = _existing.Stock.ToString();
        }

        private void SetFieldError(TextBox txt, bool hasError)
        {
            txt.BackColor = hasError
                ? Color.FromArgb(255, 220, 220)
                : (txt.ReadOnly ? Color.FromArgb(220, 220, 220) : Color.White);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            lblArticleError.Visible = false;
            lblNameError.Visible    = false;
            SetFieldError(txtArticle, false);
            SetFieldError(txtName, false);

            bool valid = true;
            if (string.IsNullOrWhiteSpace(txtArticle.Text)) { SetFieldError(txtArticle, true); lblArticleError.Text = "Поле обязательно для заполнения"; lblArticleError.Visible = true; valid = false; }
            if (string.IsNullOrWhiteSpace(txtName.Text))    { SetFieldError(txtName, true); lblNameError.Text = "Поле обязательно для заполнения"; lblNameError.Visible = true; valid = false; }
            if (cmbCategory.SelectedIndex < 0)              { valid = false; }
            if (!valid) return;

            try
            {
                using var db = new AppDbContext();
                var category = db.Categories.FirstOrDefault(c => c.Name == cmbCategory.SelectedItem!.ToString());
                if (category == null) return;

                if (_existing == null)
                {
                    if (db.Products.Any(p => p.Article == txtArticle.Text.Trim()))
                    {
                        lblArticleError.Text    = "Артикул уже существует";
                        lblArticleError.Visible = true;
                        SetFieldError(txtArticle, true);
                        return;
                    }
                    db.Products.Add(new Product
                    {
                        Article       = txtArticle.Text.Trim(),
                        Name          = txtName.Text.Trim(),
                        CategoryId    = category.Id,
                        Unit          = cmbUnit.SelectedItem!.ToString()!,
                        PurchasePrice = nudPrice.Value,
                        Stock         = 0
                    });
                }
                else
                {
                    var p = db.Products.Find(_existing.Id);
                    if (p == null) return;
                    p.Name          = txtName.Text.Trim();
                    p.CategoryId    = category.Id;
                    p.Unit          = cmbUnit.SelectedItem!.ToString()!;
                    p.PurchasePrice = nudPrice.Value;
                }

                db.SaveChanges();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
