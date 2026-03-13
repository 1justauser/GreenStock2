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

        private Label         lblArticle, lblName, lblCategory, lblUnit, lblPrice, lblExpiry;
        private TextBox       txtArticle, txtName;
        private ComboBox      cmbCategory, cmbUnit;
        private NumericUpDown nudPrice;
        private DateTimePicker dtpExpiry;
        private CheckBox      chkNoExpiry;
        private Button        btnSave, btnCancel;
        private Label         lblError, lblRequired;

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

            var screen = Screen.PrimaryScreen!.WorkingArea;
            int W = screen.Width  / 2;
            int H = screen.Height / 2;

            this.Text            = isEdit ? "Редактировать товар" : "Добавить товар";
            this.ClientSize      = new Size(W, H);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.White;

            int fontSize = W / 60;
            int labelX   = W / 8;
            int inputX   = W / 2 - 20;
            int inputW   = W / 2 - 20;
            int rowGap   = H / 9;
            int startY   = H / 12;

            Label MakeLabel(string text, int row) => new Label
            {
                Text     = text,
                Font     = new Font("Segoe UI", fontSize),
                Location = new Point(labelX, startY + row * rowGap + 3),
                AutoSize = true
            };

            lblArticle  = MakeLabel("Артикул*:", 0);
            txtArticle  = new TextBox { Font = new Font("Segoe UI", fontSize), Location = new Point(inputX, startY + 0 * rowGap), Size = new Size(inputW, H / 14), ReadOnly = isEdit, BackColor = isEdit ? Color.LightGray : Color.White };

            lblName     = MakeLabel("Название*:", 1);
            txtName     = new TextBox { Font = new Font("Segoe UI", fontSize), Location = new Point(inputX, startY + 1 * rowGap), Size = new Size(inputW, H / 14) };

            lblCategory = MakeLabel("Категория*:", 2);
            cmbCategory = new ComboBox { Font = new Font("Segoe UI", fontSize), Location = new Point(inputX, startY + 2 * rowGap), Size = new Size(inputW, H / 14), DropDownStyle = ComboBoxStyle.DropDownList };

            lblUnit     = MakeLabel("Единица изм.:", 3);
            cmbUnit     = new ComboBox { Font = new Font("Segoe UI", fontSize), Location = new Point(inputX, startY + 3 * rowGap), Size = new Size(W / 5, H / 14), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUnit.Items.AddRange(new[] { "sht", "pak", "kg", "l", "g" });
            cmbUnit.SelectedIndex = 0;

            lblPrice    = MakeLabel("Цена закупки:", 4);
            nudPrice    = new NumericUpDown { Font = new Font("Segoe UI", fontSize), Location = new Point(inputX, startY + 4 * rowGap), Size = new Size(W / 5, H / 14), Minimum = 0, Maximum = 999999, DecimalPlaces = 2 };
            var lblRub  = new Label { Text = "руб.", Font = new Font("Segoe UI", fontSize), Location = new Point(inputX + W / 5 + 8, startY + 4 * rowGap + 3), AutoSize = true };

            lblExpiry   = MakeLabel("Срок годности:", 5);
            dtpExpiry   = new DateTimePicker { Font = new Font("Segoe UI", fontSize), Location = new Point(inputX, startY + 5 * rowGap), Size = new Size(W / 4, H / 14), Format = DateTimePickerFormat.Short };
            chkNoExpiry = new CheckBox { Text = "Бессрочно", Font = new Font("Segoe UI", fontSize), Location = new Point(inputX + W / 4 + 10, startY + 5 * rowGap + 3), AutoSize = true };
            chkNoExpiry.CheckedChanged += (s, e) => dtpExpiry.Enabled = !chkNoExpiry.Checked;

            lblRequired = new Label { Text = "* — обязательное поле", Font = new Font("Segoe UI", fontSize - 1), ForeColor = Color.Gray, Location = new Point(labelX, startY + 6 * rowGap), AutoSize = true };

            int btnY = H - H / 5;
            btnSave = new Button { Text = "сохранить", Font = new Font("Segoe UI", fontSize, FontStyle.Bold), Location = new Point(inputX - 20, btnY), Size = new Size(W / 4, H / 10), BackColor = Color.FromArgb(28, 42, 74), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.AcceptButton = btnSave;

            btnCancel = new Button { Text = "отмена", Font = new Font("Segoe UI", fontSize), Location = new Point(inputX + W / 4, btnY), Size = new Size(W / 6, H / 10), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.CancelButton = btnCancel;

            lblError = new Label { Text = "", ForeColor = Color.Red, Font = new Font("Segoe UI", fontSize - 1), Location = new Point(labelX, btnY + H / 10 + 5), Size = new Size(W - labelX * 2, 20) };

            this.Controls.AddRange(new Control[]
            {
                lblArticle, txtArticle, lblName, txtName,
                lblCategory, cmbCategory, lblUnit, cmbUnit,
                lblPrice, nudPrice, lblRub,
                lblExpiry, dtpExpiry, chkNoExpiry,
                lblRequired, btnSave, btnCancel, lblError
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
            if (_existing.ExpiryDate.HasValue)
                dtpExpiry.Value = _existing.ExpiryDate.Value.ToDateTime(TimeOnly.MinValue);
            else
                chkNoExpiry.Checked = true;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            lblError.Text = "";
            if (string.IsNullOrWhiteSpace(txtArticle.Text) || string.IsNullOrWhiteSpace(txtName.Text) || cmbCategory.SelectedIndex < 0)
            { lblError.Text = "Заполните обязательные поля"; return; }

            try
            {
                using var db = new AppDbContext();
                var category = db.Categories.FirstOrDefault(c => c.Name == cmbCategory.SelectedItem!.ToString());
                if (category == null) { lblError.Text = "Категория не найдена"; return; }

                DateOnly? expiry = chkNoExpiry.Checked ? null : DateOnly.FromDateTime(dtpExpiry.Value);

                if (_existing == null)
                {
                    if (db.Products.Any(p => p.Article == txtArticle.Text.Trim()))
                    { lblError.Text = "Артикул уже существует"; return; }

                    db.Products.Add(new Product
                    {
                        Article = txtArticle.Text.Trim(), Name = txtName.Text.Trim(),
                        CategoryId = category.Id, Unit = cmbUnit.SelectedItem!.ToString()!,
                        PurchasePrice = nudPrice.Value, Stock = 0, ExpiryDate = expiry
                    });
                }
                else
                {
                    var p = db.Products.Find(_existing.Id);
                    if (p == null) return;
                    p.Name = txtName.Text.Trim(); p.CategoryId = category.Id;
                    p.Unit = cmbUnit.SelectedItem!.ToString()!; p.PurchasePrice = nudPrice.Value;
                    p.ExpiryDate = expiry;
                }

                db.SaveChanges();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex) { lblError.Text = $"Ошибка: {ex.Message}"; }
        }
    }
}
