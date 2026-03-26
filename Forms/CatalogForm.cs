using GreenStock.Data;
using GreenStock.Logging;
using GreenStock.Models;
using GreenStock.Resources;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GreenStock.Forms;

/// <summary>
/// Главная форма каталога товаров.
/// Отображает список товаров с фильтрацией, управление (добавление/редактирование/удаление)
/// доступно только администратору.
/// </summary>
public class CatalogForm : Form
{
    private static readonly ILogger _log = AppLogger.For<CatalogForm>();

    private readonly User _currentUser;

    private MenuStrip         _menuStrip      = null!;
    private ToolStripMenuItem _menuCatalog    = null!;
    private ToolStripMenuItem _menuCategories = null!;
    private ToolStripMenuItem _menuShipments  = null!;
    private ToolStripMenuItem _menuHistory    = null!;
    private ToolStripMenuItem _menuExit       = null!;
    private Label             _lblSearch      = null!;
    private Label             _lblCategory    = null!;
    private TextBox           _txtSearch      = null!;
    private ComboBox          _cmbCategory    = null!;
    private Button            _btnAdd         = null!;
    private Button            _btnEdit        = null!;
    private Button            _btnDelete      = null!;
    private Label             _lblAdminOnly   = null!;
    private DataGridView      _dgvProducts    = null!;
    private Label             _lblCount       = null!;

    private List<Product> _allProducts = new();

    /// <summary>
    /// Инициализирует форму каталога для заданного пользователя.
    /// </summary>
    /// <param name="currentUser">Текущий авторизованный пользователь.</param>
    public CatalogForm(User currentUser)
    {
        _currentUser = currentUser;
        InitializeComponent();
        ApplyRolePermissions();
        LoadData();
    }

    private void InitializeComponent()
    {
        var roleDisplay = _currentUser.Role == UserRole.Admin
            ? Strings.Role_Admin
            : Strings.Role_Kladovshik;

        Text          = Strings.Catalog_Title(_currentUser.Login, roleDisplay);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor     = Color.White;
        WindowState   = FormWindowState.Maximized;
        Size          = new Size(1200, 700);

        // ── MenuStrip ─────────────────────────────────────────
        _menuStrip      = new MenuStrip { BackColor = Color.FromArgb(28, 42, 74), ForeColor = Color.White, Font = new Font("Segoe UI", 11) };
        _menuCatalog    = new ToolStripMenuItem(Strings.Catalog_MenuCatalog)    { ForeColor = Color.White };
        _menuCategories = new ToolStripMenuItem(Strings.Catalog_MenuCategories) { ForeColor = Color.White };
        _menuShipments  = new ToolStripMenuItem(Strings.Catalog_MenuShipments)  { ForeColor = Color.White };
        _menuHistory    = new ToolStripMenuItem(Strings.Catalog_MenuHistory)    { ForeColor = Color.White };
        _menuExit       = new ToolStripMenuItem(Strings.Catalog_MenuExit)       { ForeColor = Color.White };

        _menuCategories.Click += (s, e) => OpenCategoryForm();
        _menuShipments.Click  += (s, e) => OpenShipmentForm();
        _menuHistory.Click    += (s, e) => OpenHistoryForm();
        _menuExit.Click       += (s, e) => Close();

        _menuStrip.Items.AddRange(new ToolStripItem[]
            { _menuCatalog, _menuCategories, _menuShipments, _menuHistory, _menuExit });
        MainMenuStrip = _menuStrip;

        // ── Search row ────────────────────────────────────────
        _lblSearch = new Label { Text = Strings.Catalog_LabelSearch,   Font = new Font("Segoe UI", 10), Location = new Point(12, 36),  AutoSize = true };
        _txtSearch = new TextBox { Font = new Font("Segoe UI", 10), Location = new Point(70, 33),  Size = new Size(180, 26), BorderStyle = BorderStyle.FixedSingle };
        _txtSearch.TextChanged += (s, e) => FilterGrid();

        _lblCategory = new Label { Text = Strings.Catalog_LabelCategory, Font = new Font("Segoe UI", 10), Location = new Point(270, 36), AutoSize = true };
        _cmbCategory = new ComboBox { Font = new Font("Segoe UI", 10), Location = new Point(355, 33), Size = new Size(160, 26), DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbCategory.SelectedIndexChanged += (s, e) => FilterGrid();

        // ── Buttons ───────────────────────────────────────────
        _btnAdd = new Button
        {
            Text      = Strings.Catalog_BtnAdd,
            Font      = new Font("Segoe UI", 10),
            Location  = new Point(12, 68),
            Size      = new Size(150, 30),
            BackColor = Color.FromArgb(40, 120, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnAdd.FlatAppearance.BorderSize = 0;
        _btnAdd.Click += BtnAdd_Click;

        _btnEdit = new Button
        {
            Text      = Strings.Catalog_BtnEdit,
            Font      = new Font("Segoe UI", 10),
            Location  = new Point(172, 68),
            Size      = new Size(140, 30),
            BackColor = Color.FromArgb(28, 42, 74),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnEdit.FlatAppearance.BorderSize = 0;
        _btnEdit.Click += BtnEdit_Click;

        _btnDelete = new Button
        {
            Text      = Strings.Catalog_BtnDelete,
            Font      = new Font("Segoe UI", 10),
            Location  = new Point(322, 68),
            Size      = new Size(110, 30),
            BackColor = Color.FromArgb(200, 50, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnDelete.FlatAppearance.BorderSize = 0;
        _btnDelete.Click += BtnDelete_Click;

        _lblAdminOnly = new Label
        {
            Text     = Strings.Catalog_AdminOnly,
            Font     = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            AutoSize  = true,
            Location  = new Point(442, 75),
            Visible   = false
        };

        // ── DataGridView ──────────────────────────────────────
        _dgvProducts = new DataGridView
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
        _dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 42, 74);
        _dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _dgvProducts.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 10, FontStyle.Bold);
        _dgvProducts.EnableHeadersVisualStyles = false;

        _lblCount = new Label
        {
            Text     = Strings.Catalog_CountLabel(0),
            Font     = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            AutoSize  = true,
            Location  = new Point(14, 635),
            Anchor    = AnchorStyles.Bottom | AnchorStyles.Left
        };

        Controls.AddRange(new Control[]
        {
            _menuStrip, _lblSearch, _txtSearch, _lblCategory, _cmbCategory,
            _btnAdd, _btnEdit, _btnDelete, _lblAdminOnly, _dgvProducts, _lblCount
        });
    }

    private void ApplyRolePermissions()
    {
        var isAdmin = _currentUser.Role == UserRole.Admin;
        var isKlad  = _currentUser.Role == UserRole.Kladovshik;

        _menuCategories.Visible = isAdmin;
        _menuHistory.Visible    = isAdmin;
        _menuShipments.Visible  = isKlad;

        _btnAdd.Enabled       = isAdmin;
        _btnEdit.Enabled      = isAdmin;
        _btnDelete.Enabled    = isAdmin;
        _lblAdminOnly.Visible = isKlad;
    }

    /// <summary>
    /// Загружает данные из БД и обновляет таблицу.
    /// </summary>
    public void LoadData()
    {
        try
        {
            using var db = new AppDbContext();

            var categories = db.Categories.OrderBy(c => c.Name).ToList();
            _cmbCategory.Items.Clear();
            _cmbCategory.Items.Add(Strings.Catalog_AllCategories);
            foreach (var cat in categories) _cmbCategory.Items.Add(cat.Name);
            if (_cmbCategory.SelectedIndex < 0) _cmbCategory.SelectedIndex = 0;

            _allProducts = db.Products.Include(p => p.Category).OrderBy(p => p.Article).ToList();
            _log.Debug("Загружено {0} товаров", _allProducts.Count);
            FilterGrid();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Ошибка загрузки каталога");
            MessageBox.Show($"{Strings.Catalog_ErrLoading}\n{ex.Message}",
                Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FilterGrid()
    {
        var search   = _txtSearch.Text.Trim().ToLower();
        var category = _cmbCategory.SelectedItem?.ToString() ?? Strings.Catalog_AllCategories;

        var filtered = _allProducts.AsEnumerable();
        if (!string.IsNullOrEmpty(search))
            filtered = filtered.Where(p =>
                p.Article.ToLower().Contains(search) ||
                p.Name.ToLower().Contains(search));
        if (category != Strings.Catalog_AllCategories)
            filtered = filtered.Where(p => p.Category.Name == category);

        var list = filtered.ToList();
        _dgvProducts.DataSource = list.Select(p => new
        {
            Артикул       = p.Article,
            Название      = p.Name,
            Категория     = p.Category.Name,
            Ед_изм        = p.Unit,
            Цена_руб      = p.PurchasePrice,
            Остаток       = p.Stock,
            Срок_годности = p.ExpiryDate.HasValue
                ? p.ExpiryDate.Value.ToString("dd.MM.yyyy")
                : Strings.Catalog_Perpetual,
            _Id = p.Id
        }).ToList();

        if (_dgvProducts.Columns.Contains("_Id"))        _dgvProducts.Columns["_Id"]!.Visible     = false;
        if (_dgvProducts.Columns.Contains("Ед_изм"))     _dgvProducts.Columns["Ед_изм"]!.HeaderText = Strings.Catalog_ColUnit;
        if (_dgvProducts.Columns.Contains("Цена_руб"))   _dgvProducts.Columns["Цена_руб"]!.HeaderText = Strings.Catalog_ColPrice;
        if (_dgvProducts.Columns.Contains("Срок_годности")) _dgvProducts.Columns["Срок_годности"]!.HeaderText = Strings.Catalog_ColExpiry;

        _lblCount.Text = Strings.Catalog_CountLabel(list.Count);
    }

    /// <summary>
    /// Возвращает Guid выбранного товара или <c>null</c>, если ничего не выбрано.
    /// </summary>
    private Guid? GetSelectedId()
    {
        if (_dgvProducts.CurrentRow == null) return null;
        var val = _dgvProducts.CurrentRow.Cells["_Id"].Value;
        return val is Guid id ? id : null;
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        var form = new ProductForm(null);
        if (form.ShowDialog() == DialogResult.OK) LoadData();
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        var id = GetSelectedId();
        if (id == null)
        {
            MessageBox.Show(Strings.Catalog_SelectProduct, Strings.Warning,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var db = new AppDbContext();
        var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
        if (product == null) return;

        var form = new ProductForm(product);
        if (form.ShowDialog() == DialogResult.OK) LoadData();
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        var id = GetSelectedId();
        if (id == null)
        {
            MessageBox.Show(Strings.Catalog_SelectProduct, Strings.Warning,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var db = new AppDbContext();
        var product = db.Products.FirstOrDefault(p => p.Id == id);
        if (product == null) return;

        var dlg = new DeleteConfirmForm($"{product.Article} — {product.Name}");
        if (dlg.ShowDialog() == DialogResult.Yes)
        {
            _log.Info("Удаление товара: {0} ({1})", product.Article, product.Name);
            db.Products.Remove(product);
            db.SaveChanges();
            LoadData();
        }
    }

    private void OpenCategoryForm()
    {
        new CategoryForm().ShowDialog();
        LoadData();
    }

    private void OpenShipmentForm() { new ShipmentForm(_currentUser).ShowDialog(); LoadData(); }
    private void OpenHistoryForm()  { new HistoryForm().ShowDialog(); }
}
