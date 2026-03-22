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

        private Label         lblRecipient, lblRecipientError;
        private TextBox       txtRecipient;
        private GroupBox      grpAdd;
        private Label         lblProduct, lblQty, lblAvailableLabel, lblAvailableValue;
        private ComboBox      cmbProduct;
        private NumericUpDown nudQty;
        private Button        btnAddRow;
        private DataGridView  dgvItems;
        private Label         lblWarning;
        private Button        btnConfirm, btnCancel;

        public ShipmentForm(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text            = "Новая отгрузка";
            this.Size            = new Size(620, 500);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = Color.FromArgb(240, 240, 245);

            // ── Получатель* ───────────────────────────────────
            lblRecipient = new Label { Text = "Получатель*:", Font = new Font("Segoe UI", 10), Location = new Point(15, 20), AutoSize = true };
            txtRecipient = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(125, 17), Size = new Size(200, 24) };
            lblRecipientError = new Label { Text = "Поле обязательно для заполнения", Font = new Font("Segoe UI", 8), ForeColor = Color.Red, Location = new Point(125, 44), AutoSize = true, Visible = false };

            // ── GroupBox "добавить позицию" ───────────────────
            grpAdd = new GroupBox { Text = "добавить позицию", Font = new Font("Segoe UI", 9), Location = new Point(10, 65), Size = new Size(585, 130), BackColor = Color.FromArgb(240, 240, 245) };

            lblProduct = new Label { Text = "Товар:", Font = new Font("Segoe UI", 10), Location = new Point(15, 28), AutoSize = true };
            cmbProduct = new ComboBox { Font = new Font("Segoe UI", 10), Location = new Point(80, 25), Size = new Size(280, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbProduct.SelectedIndexChanged += CmbProduct_Changed;

            lblQty            = new Label { Text = "Количество:", Font = new Font("Segoe UI", 10), Location = new Point(15, 65), AutoSize = true };
            nudQty            = new NumericUpDown { Font = new Font("Segoe UI", 10), Location = new Point(100, 62), Size = new Size(100, 24), Minimum = 1, Maximum = 999999, Value = 1 };
            nudQty.ValueChanged += (s, e) => CheckStock();
            lblAvailableLabel = new Label { Text = "доступно на складе:", Font = new Font("Segoe UI", 10), Location = new Point(210, 65), AutoSize = true };
            lblAvailableValue = new Label { Text = "", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(30, 100, 30), Location = new Point(370, 65), AutoSize = true };

            btnAddRow = new Button { Text = "+ Добавить строку", Font = new Font("Segoe UI", 10), Location = new Point(15, 96), Size = new Size(150, 28), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnAddRow.FlatAppearance.BorderColor = Color.Gray;
            btnAddRow.FlatAppearance.BorderSize  = 1;
            btnAddRow.Click += BtnAddRow_Click;

            grpAdd.Controls.AddRange(new Control[] { lblProduct, cmbProduct, lblQty, nudQty, lblAvailableLabel, lblAvailableValue, btnAddRow });

            // ── DataGridView ──────────────────────────────────
            dgvItems = new DataGridView
            {
                Location              = new Point(10, 205),
                Size                  = new Size(585, 120),
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor       = Color.White,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                Font                  = new Font("Segoe UI", 9)
            };
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(220, 230, 240);
            dgvItems.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvItems.EnableHeadersVisualStyles = false;

            // ── Warning ───────────────────────────────────────
            lblWarning = new Label { Text = "", ForeColor = Color.Red, Font = new Font("Segoe UI", 9), Location = new Point(10, 332), Size = new Size(500, 18) };

            // ── Buttons ───────────────────────────────────────
            btnConfirm = new Button
            {
                Text      = "Подтвердить",
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Location  = new Point(385, 355),
                Size      = new Size(120, 34),
                BackColor = Color.FromArgb(28, 42, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += BtnConfirm_Click;

            btnCancel = new Button
            {
                Text      = "Отмена",
                Font      = new Font("Segoe UI", 10),
                Location  = new Point(515, 355),
                Size      = new Size(80, 34),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[]
            {
                lblRecipient, txtRecipient, lblRecipientError,
                grpAdd, dgvItems, lblWarning,
                btnConfirm, btnCancel
            });
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
            lblAvailableValue.Text      = p != null ? $"{p.Stock} {p.Unit}" : "";
            lblAvailableValue.ForeColor = (p != null && p.Stock > 0) ? Color.FromArgb(30, 100, 30) : Color.Red;
            CheckStock();
        }

        private void CheckStock()
        {
            var p = SelectedProduct();
            if (p == null) return;
            lblWarning.Text = nudQty.Value <= p.Stock ? "" :
                $"Недостаточно «{p.Name}»: запрошено {nudQty.Value}, в наличии {p.Stock}";
            UpdateConfirmButton();
        }

        private void UpdateConfirmButton()
        {
            bool anyInvalid = _rows.Any(r =>
            {
                var p = _products.FirstOrDefault(x => x.Id == r.ProductId);
                return p == null || r.Quantity > p.Stock;
            });
            btnConfirm.Enabled   = _rows.Count > 0 && !anyInvalid;
            btnConfirm.BackColor = btnConfirm.Enabled ? Color.FromArgb(28, 42, 74) : Color.Gray;
        }

        private void BtnAddRow_Click(object? sender, EventArgs e)
        {
            var product = SelectedProduct();
            if (product == null) return;
            int qty = (int)nudQty.Value;
            if (qty > product.Stock)
            {
                lblWarning.Text = $"Недостаточно «{product.Name}»: запрошено {qty}, в наличии {product.Stock}";
                return;
            }

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
            if (dgvItems.Columns.Contains("Ед_изм"))             dgvItems.Columns["Ед_изм"]!.HeaderText             = "Ед. изм.";
            if (dgvItems.Columns.Contains("Доступно_на_складе")) dgvItems.Columns["Доступно_на_складе"]!.HeaderText = "Доступно на складе";
        }

        private void BtnConfirm_Click(object? sender, EventArgs e)
        {
            lblRecipientError.Visible = false;
            txtRecipient.BackColor    = Color.White;

            if (string.IsNullOrWhiteSpace(txtRecipient.Text))
            {
                lblRecipientError.Visible = true;
                txtRecipient.BackColor    = Color.FromArgb(255, 220, 220);
                return;
            }
            if (_rows.Count == 0) { lblWarning.Text = "Добавьте хотя бы одну позицию"; return; }

            try
            {
                using var db = new AppDbContext();
                using var tx = db.Database.BeginTransaction();

                foreach (var row in _rows)
                {
                    var product = db.Products.Find(row.ProductId);
                    if (product == null || row.Quantity > product.Stock)
                    {
                        lblWarning.Text = $"Недостаточно «{row.ProductName}»: в наличии {product?.Stock ?? 0}";
                        tx.Rollback();
                        return;
                    }
                }

                var shipment = new Shipment
                {
                    CreatedBy = _currentUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    Recipient = txtRecipient.Text.Trim()
                };
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
