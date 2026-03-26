using GreenStock.Data;
using GreenStock.Logging;
using GreenStock.Models;
using GreenStock.Resources;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GreenStock.Forms;

/// <summary>
/// Форма создания новой отгрузки товаров.
/// Доступна только кладовщику.
/// </summary>
public class ShipmentForm : Form
{
    private static readonly ILogger _log = AppLogger.For<ShipmentForm>();

    private readonly User _currentUser;

    /// <summary>
    /// Строка позиции в таблице создаваемой отгрузки.
    /// </summary>
    private class ShipmentRow
    {
        /// <summary>Идентификатор товара (UUID).</summary>
        public Guid   ProductId   { get; set; }
        /// <summary>Название товара.</summary>
        public string ProductName { get; set; } = string.Empty;
        /// <summary>Единица измерения.</summary>
        public string Unit        { get; set; } = string.Empty;
        /// <summary>Запрошенное количество.</summary>
        public int    Quantity    { get; set; }
        /// <summary>Текущий остаток на складе.</summary>
        public int    Available   { get; set; }
    }

    private readonly List<ShipmentRow> _rows = new();
    private List<Product>              _products = new();

    private Label         _lblRecipient      = null!;
    private Label         _lblRecipientError = null!;
    private TextBox       _txtRecipient      = null!;
    private GroupBox      _grpAdd            = null!;
    private Label         _lblProduct        = null!;
    private Label         _lblQty            = null!;
    private Label         _lblAvailableLabel = null!;
    private Label         _lblAvailableValue = null!;
    private ComboBox      _cmbProduct        = null!;
    private NumericUpDown _nudQty            = null!;
    private Button        _btnAddRow         = null!;
    private DataGridView  _dgvItems          = null!;
    private Label         _lblWarning        = null!;
    private Button        _btnConfirm        = null!;
    private Button        _btnCancel         = null!;

    /// <summary>
    /// Инициализирует форму отгрузки для заданного пользователя.
    /// </summary>
    /// <param name="currentUser">Текущий авторизованный пользователь (кладовщик).</param>
    public ShipmentForm(User currentUser)
    {
        _currentUser = currentUser;
        InitializeComponent();
        LoadProducts();
    }

    private void InitializeComponent()
    {
        Text            = Strings.Shipment_Title;
        Size            = new Size(620, 500);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = Color.FromArgb(240, 240, 245);

        _lblRecipient = new Label
            { Text = Strings.Shipment_LabelRecipient, Font = new Font("Segoe UI", 10), Location = new Point(15, 20), AutoSize = true };
        _txtRecipient = new TextBox
            { Font = new Font("Segoe UI", 10), Location = new Point(125, 17), Size = new Size(200, 24) };
        _lblRecipientError = new Label
        {
            Text     = Strings.RequiredField,
            Font     = new Font("Segoe UI", 8),
            ForeColor = Color.Red,
            Location  = new Point(125, 44),
            AutoSize  = true,
            Visible   = false
        };

        _grpAdd = new GroupBox
            { Text = Strings.Shipment_GroupAdd, Font = new Font("Segoe UI", 9), Location = new Point(10, 65), Size = new Size(585, 130), BackColor = Color.FromArgb(240, 240, 245) };

        _lblProduct = new Label
            { Text = Strings.Shipment_LabelProduct, Font = new Font("Segoe UI", 10), Location = new Point(15, 28), AutoSize = true };
        _cmbProduct = new ComboBox
            { Font = new Font("Segoe UI", 10), Location = new Point(80, 25), Size = new Size(280, 24), DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbProduct.SelectedIndexChanged += CmbProduct_Changed;

        _lblQty            = new Label { Text = Strings.Shipment_LabelQty,       Font = new Font("Segoe UI", 10), Location = new Point(15, 65),  AutoSize = true };
        _nudQty            = new NumericUpDown { Font = new Font("Segoe UI", 10), Location = new Point(100, 62), Size = new Size(100, 24), Minimum = 1, Maximum = 999999, Value = 1 };
        _nudQty.ValueChanged += (s, e) => CheckStock();
        _lblAvailableLabel = new Label { Text = Strings.Shipment_LabelAvailable,  Font = new Font("Segoe UI", 10), Location = new Point(210, 65), AutoSize = true };
        _lblAvailableValue = new Label { Text = string.Empty, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(30, 100, 30), Location = new Point(370, 65), AutoSize = true };

        _btnAddRow = new Button
        {
            Text      = Strings.Shipment_BtnAddRow,
            Font      = new Font("Segoe UI", 10),
            Location  = new Point(15, 96),
            Size      = new Size(150, 28),
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnAddRow.FlatAppearance.BorderColor = Color.Gray;
        _btnAddRow.FlatAppearance.BorderSize  = 1;
        _btnAddRow.Click += BtnAddRow_Click;

        _grpAdd.Controls.AddRange(new Control[]
            { _lblProduct, _cmbProduct, _lblQty, _nudQty, _lblAvailableLabel, _lblAvailableValue, _btnAddRow });

        _dgvItems = new DataGridView
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
        _dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(220, 230, 240);
        _dgvItems.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
        _dgvItems.EnableHeadersVisualStyles = false;

        _lblWarning = new Label
            { Text = string.Empty, ForeColor = Color.Red, Font = new Font("Segoe UI", 9), Location = new Point(10, 332), Size = new Size(500, 18) };

        _btnConfirm = new Button
        {
            Text      = Strings.Shipment_BtnConfirm,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Location  = new Point(385, 355),
            Size      = new Size(120, 34),
            BackColor = Color.FromArgb(28, 42, 74),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnConfirm.FlatAppearance.BorderSize = 0;
        _btnConfirm.Click += BtnConfirm_Click;

        _btnCancel = new Button
        {
            Text      = Strings.Get("Cancel"),
            Font      = new Font("Segoe UI", 10),
            Location  = new Point(515, 355),
            Size      = new Size(80, 34),
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

        Controls.AddRange(new Control[]
        {
            _lblRecipient, _txtRecipient, _lblRecipientError,
            _grpAdd, _dgvItems, _lblWarning,
            _btnConfirm, _btnCancel
        });
    }

    private void LoadProducts()
    {
        using var db = new AppDbContext();
        _products = db.Products.Include(p => p.Category).OrderBy(p => p.Name).ToList();
        _cmbProduct.Items.Clear();
        foreach (var p in _products) _cmbProduct.Items.Add($"{p.Article} — {p.Name}");
        if (_cmbProduct.Items.Count > 0) _cmbProduct.SelectedIndex = 0;
    }

    private Product? SelectedProduct() =>
        _cmbProduct.SelectedIndex < 0 ? null : _products[_cmbProduct.SelectedIndex];

    private void CmbProduct_Changed(object? sender, EventArgs e)
    {
        var p = SelectedProduct();
        _lblAvailableValue.Text      = p != null ? $"{p.Stock} {p.Unit}" : string.Empty;
        _lblAvailableValue.ForeColor = (p != null && p.Stock > 0) ? Color.FromArgb(30, 100, 30) : Color.Red;
        CheckStock();
    }

    private void CheckStock()
    {
        var p = SelectedProduct();
        if (p == null) return;
        _lblWarning.Text = _nudQty.Value <= p.Stock
            ? string.Empty
            : Strings.Shipment_ErrInsufficientStock(p.Name, _nudQty.Value, p.Stock);
        UpdateConfirmButton();
    }

    private void UpdateConfirmButton()
    {
        var anyInvalid = _rows.Any(r =>
        {
            var p = _products.FirstOrDefault(x => x.Id == r.ProductId);
            return p == null || r.Quantity > p.Stock;
        });
        _btnConfirm.Enabled   = _rows.Count > 0 && !anyInvalid;
        _btnConfirm.BackColor = _btnConfirm.Enabled ? Color.FromArgb(28, 42, 74) : Color.Gray;
    }

    private void BtnAddRow_Click(object? sender, EventArgs e)
    {
        var product = SelectedProduct();
        if (product == null) return;
        var qty = (int)_nudQty.Value;
        if (qty > product.Stock)
        {
            _lblWarning.Text = Strings.Shipment_ErrInsufficientStock(product.Name, qty, product.Stock);
            return;
        }

        var existing = _rows.FirstOrDefault(r => r.ProductId == product.Id);
        if (existing != null)
            existing.Quantity += qty;
        else
            _rows.Add(new ShipmentRow
            {
                ProductId   = product.Id,
                ProductName = product.Name,
                Unit        = product.Unit,
                Quantity    = qty,
                Available   = product.Stock
            });

        RefreshGrid();
        _nudQty.Value    = 1;
        _lblWarning.Text = string.Empty;
        UpdateConfirmButton();
    }

    private void RefreshGrid()
    {
        _dgvItems.DataSource = null;
        _dgvItems.DataSource = _rows.Select(r => new
        {
            Товар              = r.ProductName,
            Ед_изм             = r.Unit,
            Количество         = r.Quantity,
            Доступно_на_складе = r.Available
        }).ToList();

        if (_dgvItems.Columns.Contains("Ед_изм"))             _dgvItems.Columns["Ед_изм"]!.HeaderText             = Strings.Shipment_LabelQty;
        if (_dgvItems.Columns.Contains("Доступно_на_складе")) _dgvItems.Columns["Доступно_на_складе"]!.HeaderText = Strings.Shipment_LabelAvailable;
    }

    private void BtnConfirm_Click(object? sender, EventArgs e)
    {
        _lblRecipientError.Visible = false;
        _txtRecipient.BackColor    = Color.White;

        if (string.IsNullOrWhiteSpace(_txtRecipient.Text))
        {
            _lblRecipientError.Visible = true;
            _txtRecipient.BackColor    = Color.FromArgb(255, 220, 220);
            return;
        }
        if (_rows.Count == 0)
        {
            _lblWarning.Text = Strings.Shipment_ErrNoRows;
            return;
        }

        try
        {
            using var db = new AppDbContext();
            using var tx = db.Database.BeginTransaction();

            // Финальная проверка остатков перед сохранением
            foreach (var row in _rows)
            {
                var product = db.Products.Find(row.ProductId);
                if (product == null || row.Quantity > product.Stock)
                {
                    _lblWarning.Text = Strings.Shipment_ErrInsufficientStock(
                        row.ProductName, row.Quantity, product?.Stock ?? 0);
                    tx.Rollback();
                    return;
                }
            }

            var shipment = new Shipment
            {
                CreatedBy = _currentUser.Id,
                CreatedAt = DateTime.UtcNow,
                Recipient = _txtRecipient.Text.Trim()
            };
            db.Shipments.Add(shipment);
            db.SaveChanges();

            foreach (var row in _rows)
            {
                db.ShipmentItems.Add(new ShipmentItem
                {
                    ShipmentId = shipment.Id,
                    ProductId  = row.ProductId,
                    Quantity   = row.Quantity
                });
                var product = db.Products.Find(row.ProductId)!;
                product.Stock -= row.Quantity;
            }
            db.SaveChanges();
            tx.Commit();

            _log.Info("Отгрузка {0} оформлена пользователем {1}, получатель: {2}, позиций: {3}",
                shipment.Id, _currentUser.Login, shipment.Recipient, _rows.Count);

            MessageBox.Show(Strings.Shipment_Success, Strings.Done,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Ошибка при оформлении отгрузки");
            MessageBox.Show($"{Strings.Error}:\n{ex.Message}",
                Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
