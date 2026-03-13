using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GreenStock.Data;
using GreenStock.Models;
using Microsoft.EntityFrameworkCore;

namespace GreenStock.Forms
{
    public class ShipmentForm : Form
    {
        private readonly User _currentUser;

        private class ShipmentRow
        {
            public int    ProductId   { get; set; }
            public string ProductName { get; set; } = "";
            public string Unit        { get; set; } = "";
            public int    Quantity    { get; set; }
            public int    Available   { get; set; }
        }
        private readonly List<ShipmentRow> _rows = new();

        private Label          lblRecipient;
        private TextBox        txtRecipient;
        private TabControl     tabControl;
        private TabPage        tabAdd;
        private Label          lblProduct, lblQty, lblAvailableLabel, lblAvailableValue;
        private ComboBox       cmbProduct;
        private NumericUpDown  nudQty;
        private Button         btnAddRow;
        private DataGridView   dgvItems;
        private Button         btnConfirm, btnCancel;
        private Label          lblWarning;

        public ShipmentForm(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            var screen = Screen.PrimaryScreen!.WorkingArea;
            int W = screen.Width  * 3 / 4;
            int H = screen.Height * 3 / 4;

            this.Text            = "Новая отгрузка";
            this.ClientSize      = new Size(W, H);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.BackColor       = Color.White;

            int fontSize = W / 80;
            int inputW   = W / 3;
            int fieldH   = H / 16;

            // ── Получатель ────────────────────────────────────
            lblRecipient = new Label { Text = "Получатель:", Font = new Font("Segoe UI", fontSize), Location = new Point(10, 12), AutoSize = true };
            txtRecipient = new TextBox { Font = new Font("Segoe UI", fontSize), Location = new Point(110, 10), Size = new Size(inputW, fieldH) };

            // ── TabControl ────────────────────────────────────
            tabControl = new TabControl { Location = new Point(8, fieldH + 20), Size = new Size(W - 16, H / 3), Font = new Font("Segoe UI", fontSize) };
            tabAdd     = new TabPage("добавить позицию");
            tabControl.TabPages.Add(tabAdd);

            lblProduct = new Label { Text = "Товар:", Font = new Font("Segoe UI", fontSize), Location = new Point(14, 20), AutoSize = true };
            cmbProduct = new ComboBox { Font = new Font("Segoe UI", fontSize), Location = new Point(90, 17), Size = new Size(inputW, fieldH), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProduct.SelectedIndexChanged += CmbProduct_Changed;

            lblQty           = new Label { Text = "Количество:", Font = new Font("Segoe UI", fontSize), Location = new Point(14, 54), AutoSize = true };
            nudQty           = new NumericUpDown { Font = new Font("Segoe UI", fontSize), Location = new Point(110, 51), Size = new Size(80, fieldH), Minimum = 1, Maximum = 999999, Value = 1 };
            nudQty.ValueChanged += (s, e) => CheckStock();
            lblAvailableLabel = new Label { Text = "доступно на складе:", Font = new Font("Segoe UI", fontSize), Location = new Point(200, 54), AutoSize = true };
            lblAvailableValue = new Label { Text = "—", Font = new Font("Segoe UI", fontSize, FontStyle.Bold), ForeColor = Color.FromArgb(30, 100, 30), Location = new Point(360, 54), AutoSize = true };

            btnAddRow = new Button { Text = "+ Добавить строку", Font = new Font("Segoe UI", fontSize), Location = new Point(14, 88), Size = new Size(150, 30), BackColor = Color.FromArgb(28, 42, 74), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnAddRow.FlatAppearance.BorderSize = 0;
            btnAddRow.Click += BtnAddRow_Click;

            tabAdd.Controls.AddRange(new Control[] { lblProduct, cmbProduct, lblQty, nudQty, lblAvailableLabel, lblAvailableValue, btnAddRow });

            // ── Grid ──────────────────────────────────────────
            dgvItems = new DataGridView
            {
                Location              = new Point(8, fieldH + 20 + H / 3 + 8),
                Size                  = new Size(W - 16, H / 4),
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor       = Color.White,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                Font                  = new Font("Segoe UI", fontSize)
            };
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(220, 230, 245);
            dgvItems.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", fontSize, FontStyle.Bold);
            dgvItems.EnableHeadersVisualStyles = false;

            // ── Warning + Buttons ─────────────────────────────
            int bottomY = H - H / 8;
            lblWarning = new Label { Text = "", ForeColor = Color.Red, Font = new Font("Segoe UI", fontSize - 1), Location = new Point(8, bottomY), Size = new Size(W / 2, 20) };

            btnConfirm = new Button { Text = "подтвердить отгрузку", Font = new Font("Segoe UI", fontSize, FontStyle.Bold), Location = new Point(W - W / 3 - 10, bottomY - 5), Size = new Size(W / 3, H / 12), BackColor = Color.FromArgb(28, 42, 74), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += BtnConfirm_Click;

            btnCancel = new Button { Text = "отмена", Font = new Font("Segoe UI", fontSize), Location = new Point(W - W / 3 - 10 + W / 3 + 8, bottomY - 5), Size = new Size(W / 8, H / 12), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { lblRecipient, txtRecipient, tabControl, dgvItems, lblWarning, btnConfirm, btnCancel });
        }

        private List<Product> _products = new();

        private void LoadProducts()
        {
            using var db = new AppDbContext();
            _products = db.Products.Include(p => p.Category).OrderBy(p => p.Name).ToList();
            cmbProduct.Items.Clear();
            foreach (var p in _products) cmbProduct.Items.Add($"{p.Article} — {p.Name}");
            if (cmbProduct.Items.Count > 0) cmbProduct.SelectedIndex = 0;
        }

        private Product? SelectedProduct() =>
            cmbProduct.SelectedIndex < 0 ? null : _products[cmbProduct.SelectedIndex];

        private void CmbProduct_Changed(object? sender, EventArgs e)
        {
            var p = SelectedProduct();
            lblAvailableValue.Text      = p != null ? $"{p.Stock} {p.Unit}" : "—";
            lblAvailableValue.ForeColor = (p != null && p.Stock > 0) ? Color.FromArgb(30, 100, 30) : Color.Red;
            CheckStock();
        }

        private void CheckStock()
        {
            var p = SelectedProduct();
            if (p == null) return;
            lblWarning.Text = nudQty.Value <= p.Stock ? "" :
                $"Недостаточно товара {p.Name}: запрошено {nudQty.Value}, в наличии {p.Stock}";
            UpdateConfirmButton();
        }

        private void UpdateConfirmButton()
        {
            bool anyInvalid = _rows.Any(r => { var p = _products.FirstOrDefault(x => x.Id == r.ProductId); return p == null || r.Quantity > p.Stock; });
            btnConfirm.Enabled   = _rows.Count > 0 && !anyInvalid;
            btnConfirm.BackColor = btnConfirm.Enabled ? Color.FromArgb(28, 42, 74) : Color.Gray;
        }

        private void BtnAddRow_Click(object? sender, EventArgs e)
        {
            var product = SelectedProduct();
            if (product == null) return;
            int qty = (int)nudQty.Value;
            if (qty > product.Stock) { lblWarning.Text = $"Недостаточно товара {product.Name}: запрошено {qty}, в наличии {product.Stock}"; return; }

            var existing = _rows.FirstOrDefault(r => r.ProductId == product.Id);
            if (existing != null) existing.Quantity += qty;
            else _rows.Add(new ShipmentRow { ProductId = product.Id, ProductName = product.Name, Unit = product.Unit, Quantity = qty, Available = product.Stock });

            RefreshGrid();
            nudQty.Value    = 1;
            lblWarning.Text = "";
            UpdateConfirmButton();
        }

        private void RefreshGrid()
        {
            dgvItems.DataSource = null;
            dgvItems.DataSource = _rows.Select(r => new
            {
                Товар              = r.ProductName,
                Ед_изм             = r.Unit,
                Количество         = r.Quantity,
                Доступно_на_складе = r.Available
            }).ToList();
            if (dgvItems.Columns.Contains("Ед_изм"))             dgvItems.Columns["Ед_изм"]!.HeaderText = "Ед. изм.";
            if (dgvItems.Columns.Contains("Доступно_на_складе")) dgvItems.Columns["Доступно_на_складе"]!.HeaderText = "Доступно на складе";
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            if (_rows.Count == 0) { lblWarning.Text = "Добавьте хотя бы одну позицию"; return; }
            if (string.IsNullOrWhiteSpace(txtRecipient.Text)) { lblWarning.Text = "Укажите получателя"; return; }

            try
            {
                using var db = new AppDbContext();
                using var tx = db.Database.BeginTransaction();

                foreach (var row in _rows)
                {
                    var product = db.Products.Find(row.ProductId);
                    if (product == null || row.Quantity > product.Stock)
                    {
                        lblWarning.Text = $"Недостаточно товара {row.ProductName}: в наличии {product?.Stock ?? 0}";
                        tx.Rollback();
                        return;
                    }
                }

                var shipment = new Shipment { CreatedBy = _currentUser.Id, CreatedAt = DateTime.UtcNow, Recipient = txtRecipient.Text.Trim() };
                db.Shipments.Add(shipment);
                db.SaveChanges();

                foreach (var row in _rows)
                {
                    db.ShipmentItems.Add(new ShipmentItem { ShipmentId = shipment.Id, ProductId = row.ProductId, Quantity = row.Quantity });
                    var product = db.Products.Find(row.ProductId)!;
                    product.Stock -= row.Quantity;
                }
                db.SaveChanges();
                tx.Commit();

                MessageBox.Show("Отгрузка успешно оформлена!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
