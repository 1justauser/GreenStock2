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
    public class CatalogForm : Form
    {
        private readonly User _currentUser;

        private MenuStrip         menuStrip;
        private ToolStripMenuItem menuCatalog, menuShipments, menuHistory, menuExit;
        private Label             lblSearch, lblCategory;
        private TextBox           txtSearch;
        private ComboBox          cmbCategory;
        private Button            btnAdd, btnEdit, btnDelete;
        private Label             lblAdminOnly;
        private DataGridView      dgvProducts;
        private Label             lblCount;

        public CatalogForm(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            ApplyRolePermissions();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text        = $"Каталог товаров — {_currentUser.Login} ({_currentUser.Role})";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor   = Color.White;
            this.WindowState = FormWindowState.Maximized;
            this.Size        = new Size(1200, 700);

            // ── MenuStrip ─────────────────────────────────────
            menuStrip     = new MenuStrip { BackColor = Color.FromArgb(28, 42, 74), ForeColor = Color.White, Font = new Font("Segoe UI", 11) };
            menuCatalog   = new ToolStripMenuItem("Каталог")  { ForeColor = Color.White };
            menuShipments = new ToolStripMenuItem("Отгрузки") { ForeColor = Color.White };
            menuHistory   = new ToolStripMenuItem("История")  { ForeColor = Color.White };
            menuExit      = new ToolStripMenuItem("Выйти")    { ForeColor = Color.White };

            menuShipments.Click += (s, e) => OpenShipmentForm();
            menuHistory.Click   += (s, e) => OpenHistoryForm();
            menuExit.Click      += (s, e) => this.Close();

            menuStrip.Items.AddRange(new ToolStripItem[] { menuCatalog, menuShipments, menuHistory, menuExit });
            this.MainMenuStrip = menuStrip;

            // ── Search row ────────────────────────────────────
            lblSearch = new Label { Text = "Поиск:", Font = new Font("Segoe UI", 10), Location = new Point(12, 36), AutoSize = true };
            txtSearch = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(70, 33), Size = new Size(180, 26), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += (s, e) => FilterGrid();

            lblCategory = new Label { Text = "Категория:", Font = new Font("Segoe UI", 10), Location = new Point(270, 36), AutoSize = true };
            cmbCategory = new ComboBox { Font = new Font("Segoe UI", 10), Location = new Point(355, 33), Size = new Size(160, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.SelectedIndexChanged += (s, e) => FilterGrid();

            // ── Buttons ───────────────────────────────────────
            btnAdd = new Button { Text = "+ Добавить товар", Font = new Font("Segoe UI", 10), Location = new Point(12, 68), Size = new Size(150, 30), BackColor = Color.FromArgb(40, 120, 200), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button { Text = "Редактировать", Font = new Font("Segoe UI", 10), Location = new Point(172, 68), Size = new Size(140, 30), BackColor = Color.FromArgb(28, 42, 74), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button { Text = "✕ Удалить", Font = new Font("Segoe UI", 10), Location = new Point(322, 68), Size = new Size(110, 30), BackColor = Color.FromArgb(200, 50, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;

            lblAdminOnly = new Label { Text = "недоступно для кладовщика", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true, Location = new Point(442, 75), Visible = false };

            // ── DataGridView ──────────────────────────────────
            dgvProducts = new DataGridView
            {
                Location              = new Point(12, 108),
                Anchor                = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size                  = new Size(1160, 520),
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.Fixed3D,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                Font                  = new Font("Segoe UI", 10)
            };
            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 42, 74);
            dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvProducts.EnableHeadersVisualStyles = false;

            lblCount = new Label { Text = "всего позиций: 0", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, AutoSize = true, Location = new Point(14, 635), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };

            this.Controls.AddRange(new Control[] { menuStrip, lblSearch, txtSearch, lblCategory, cmbCategory, btnAdd, btnEdit, btnDelete, lblAdminOnly, dgvProducts, lblCount });
        }

        private void ApplyRolePermissions()
        {
            bool isAdmin = _currentUser.Role == "Admin";
            bool isKlad  = _currentUser.Role == "Kladovshik";

            btnAdd.Enabled       = isAdmin;
            btnEdit.Enabled      = isAdmin;
            btnDelete.Enabled    = isAdmin;
            lblAdminOnly.Visible = isKlad;
            menuShipments.Visible= isKlad;
            menuHistory.Visible  = isAdmin;
        }

        private List<Product> _allProducts = new();

        public void LoadData()
        {
            try
            {
                using var db = new AppDbContext();
                var categories = db.Categories.OrderBy(c => c.Name).ToList();
                cmbCategory.Items.Clear();
                cmbCategory.Items.Add("Все");
                foreach (var cat in categories) cmbCategory.Items.Add(cat.Name);
                if (cmbCategory.SelectedIndex < 0) cmbCategory.SelectedIndex = 0;

                _allProducts = db.Products.Include(p => p.Category).OrderBy(p => p.Article).ToList();
                FilterGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterGrid()
        {
            string search   = txtSearch.Text.Trim().ToLower();
            string category = cmbCategory.SelectedItem?.ToString() ?? "Все";

            var filtered = _allProducts.AsEnumerable();
            if (!string.IsNullOrEmpty(search))
                filtered = filtered.Where(p => p.Article.ToLower().Contains(search) || p.Name.ToLower().Contains(search));
            if (category != "Все")
                filtered = filtered.Where(p => p.Category.Name == category);

            var list = filtered.ToList();
            dgvProducts.DataSource = list.Select(p => new
            {
                Артикул       = p.Article,
                Название      = p.Name,
                Категория     = p.Category.Name,
                Ед_изм        = p.Unit,
                Цена_руб      = p.PurchasePrice,
                Остаток       = p.Stock,
                Срок_годности = p.ExpiryDate.HasValue ? p.ExpiryDate.Value.ToString("dd.MM.yyyy") : "Бессрочно",
                _Id           = p.Id
            }).ToList();

            if (dgvProducts.Columns.Contains("_Id"))           dgvProducts.Columns["_Id"]!.Visible = false;
            if (dgvProducts.Columns.Contains("Ед_изм"))        dgvProducts.Columns["Ед_изм"]!.HeaderText = "Ед. изм.";
            if (dgvProducts.Columns.Contains("Цена_руб"))      dgvProducts.Columns["Цена_руб"]!.HeaderText = "Цена (руб.)";
            if (dgvProducts.Columns.Contains("Срок_годности")) dgvProducts.Columns["Срок_годности"]!.HeaderText = "Срок годности";

            lblCount.Text = $"всего позиций: {list.Count}";
        }

        private int? GetSelectedId()
        {
            if (dgvProducts.CurrentRow == null) return null;
            return (int)dgvProducts.CurrentRow.Cells["_Id"].Value;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var form = new ProductForm(null);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            int? id = GetSelectedId();
            if (id == null) { MessageBox.Show("Выберите товар.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            using var db = new AppDbContext();
            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (product == null) return;
            var form = new ProductForm(product);
            if (form.ShowDialog() == DialogResult.OK) LoadData();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            int? id = GetSelectedId();
            if (id == null) { MessageBox.Show("Выберите товар.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            using var db = new AppDbContext();
            var product = db.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return;
            if (MessageBox.Show($"Удалить [{product.Name}]?\nЭто действие нельзя отменить.", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            { db.Products.Remove(product); db.SaveChanges(); LoadData(); }
        }

        private void OpenShipmentForm() { new ShipmentForm(_currentUser).ShowDialog(); LoadData(); }
        private void OpenHistoryForm()  { new HistoryForm().ShowDialog(); }
    }
}
