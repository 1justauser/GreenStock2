using GreenStock;
using GreenStock.Data;
using GreenStock.Logging;
using GreenStock.Models;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GreenStock.Forms;

/// <summary>
/// Форма создания новой отгрузки товаров.
/// Исправлено: правильный layout (кнопки не налезают), кнопка "Отмена" полная.
/// </summary>
public class ShipmentForm : Form
{
    private static readonly ILogger _log = AppLogger.For<ShipmentForm>();

    private readonly User _currentUser;

    private class ShipmentRow
    {
        public Guid   ProductId   { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Unit        { get; set; } = string.Empty;
        public int    Quantity    { get; set; }
        public int    Available   { get; set; }
    }

    private readonly List<ShipmentRow> _rows = new();
    private List<Product> _products = new();

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

    public ShipmentForm(User currentUser)
    {
        _currentUser = currentUser;
        InitializeComponent();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        LoadProducts();
    }

    private void InitializeComponent()
    {
        Text            = Strings.Shipment_Title;
        Size            = new Size(660, 530);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = Color.White;

        // ── Получатель ────────────────────────────────────────
        _lblRecipient = new Label
        {
            Text     = Strings.Shipment_LabelRecipient,
            Font     = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(15, 15),
            AutoSize = true
        };
        _txtRecipient = new TextBox
        {
            Font        = new Font("Segoe UI", 10),
            Location    = new Point(135, 12),
            Size        = new Size(490, 26),
            BorderStyle = BorderStyle.FixedSingle
        };
        _lblRecipientError = new Label
        {
            Text      = Strings.RequiredField,
            Font      = new Font("Segoe UI", 8),
            ForeColor = Color.Red,
            Location  = new Point(135, 41),
            AutoSize  = true,
            Visible   = false
        };

        // ── GroupBox: добавить позицию ────────────────────────
        _grpAdd = new GroupBox
        {
            Text      = Strings.Shipment_GroupAdd,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Location  = new Point(12, 60),
            Size      = new Size(620, 115),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(28, 42, 74)
        };

        _lblProduct = new Label
        {
            Text     = Strings.Shipment_LabelProduct,
            Font     = new Font("Segoe UI", 10),
            Location = new Point(12, 28),
            AutoSize = true
        };
        _cmbProduct = new ComboBox
        {
            Font          = new Font("Segoe UI", 10),
            Location      = new Point(80, 25),
            Size          = new Size(300, 26),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbProduct.SelectedIndexChanged += CmbProduct_Changed;

        _lblQty = new Label
        {
            Text     = "Кол-во:",
            Font     = new Font("Segoe UI", 10),
            Location = new Point(390, 28),
            AutoSize = true
        };
        _nudQty = new NumericUpDown
        {
            Font     = new Font("Segoe UI", 10),
            Location = new Point(440, 25),
            Size     = new Size(80, 26),
            Minimum  = 1,
            Maximum  = 999999,
            Value    = 1
        };
        _nudQty.ValueChanged += (s, e) => CheckStock();

        _lblAvailableLabel = new Label
        {
            Text     = Strings.Shipment_LabelAvailable,
            Font     = new Font("Segoe UI", 10),
            Location = new Point(12, 65),
            AutoSize = true
        };
        _lblAvailableValue = new Label
        {
            Text      = string.Empty,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 100, 30),
            Location  = new Point(160, 65),
            AutoSize  = true
        };

        _btnAddRow = new Button
        {
            Text      = Strings.Shipment_BtnAddRow,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Location  = new Point(390, 58),
            Size      = new Size(200, 34),
            BackColor = Color.FromArgb(40, 120, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnAddRow.FlatAppearance.BorderSize = 0;
        _btnAddRow.Click += BtnAddRow_Click;

        _grpAdd.Controls.AddRange(new Control[]
            { _lblProduct, _cmbProduct, _lblQty, _nudQty,
              _lblAvailableLabel, _lblAvailableValue, _btnAddRow });

        // ── Таблица позиций ───────────────────────────────────
        _dgvItems = new DataGridView
        {
            Location              = new Point(12, 185),
            Size                  = new Size(620, 220),
            ReadOnly              = true,
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor       = Color.White,
            BorderStyle           = BorderStyle.Fixed3D,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            Font                  = new Font("Segoe UI", 9),
            RowHeadersVisible     = false
        };
        _dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 42, 74);
        _dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _dgvItems.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
        _dgvItems.EnableHeadersVisualStyles = false;
        _dgvItems.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 250);

        // ── Предупреждение ─────────────────────────────────────
        _lblWarning = new Label
        {
            Text      = string.Empty,
            ForeColor = Color.Red,
            Font      = new Font("Segoe UI", 9),
            Location  = new Point(12, 412),
            Size      = new Size(620, 20)
        };

        // ── Кнопки подтвердить / отмена ───────────────────────
        // Располагаем правильно: Подтвердить (160px) + отступ (8px) + Отмена (100px)
        // Правый край формы: 620+12=632 → правая кнопка заканчивается на 632
        _btnCancel = new Button
        {
            Text      = "Отмена",
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Location  = new Point(520, 440),
            Size      = new Size(112, 38),
            BackColor = Color.FromArgb(200, 50, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnCancel.FlatAppearance.BorderSize = 0;
        _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

        _btnConfirm = new Button
        {
            Text      = Strings.Shipment_BtnConfirm,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Location  = new Point(350, 440),
            Size      = new Size(162, 38),
            BackColor = Color.Gray,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand,
            Enabled   = false
        };
        _btnConfirm.FlatAppearance.BorderSize = 0;
        _btnConfirm.Click += BtnConfirm_Click;

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
        var today = DateOnly.FromDateTime(DateTime.Today);
        // Показываем только товары с ненулевым остатком и не просроченные
        _products = db.Products
            .Include(p => p.Category)
            .Where(p => p.Stock > 0 && (p.ExpiryDate == null || p.ExpiryDate >= today))
            .OrderBy(p => p.Name)
            .ToList();
        _cmbProduct.Items.Clear();
        foreach (var p in _products)
            _cmbProduct.Items.Add($"{p.Article} — {p.Name}");
        if (_cmbProduct.Items.Count > 0) _cmbProduct.SelectedIndex = 0;
    }

    private Product? SelectedProduct() =>
        _cmbProduct.SelectedIndex < 0 ? null : _products[_cmbProduct.SelectedIndex];

    private void CmbProduct_Changed(object? sender, EventArgs e)
    {
        var p = SelectedProduct();
        _lblAvailableValue.Text      = p != null ? $"{p.Stock} {p.Unit}" : string.Empty;
        _lblAvailableValue.ForeColor = (p != null && p.Stock > 0)
            ? Color.FromArgb(30, 100, 30)
            : Color.Red;
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
        _btnConfirm.BackColor = _btnConfirm.Enabled
            ? Color.FromArgb(28, 42, 74)
            : Color.Gray;
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
            Товар      = r.ProductName,
            Ед_изм     = r.Unit,
            Количество = r.Quantity,
            На_складе  = r.Available
        }).ToList();

        if (_dgvItems.Columns.Contains("Ед_изм"))    _dgvItems.Columns["Ед_изм"]!.HeaderText    = "Ед. изм.";
        if (_dgvItems.Columns.Contains("На_складе")) _dgvItems.Columns["На_складе"]!.HeaderText = "Доступно";
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
            using var db  = new AppDbContext();
            using var tx  = db.Database.BeginTransaction();

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
                var product = db.Products.Find(row.ProductId)!;
                // Используем цену продажи если задана, иначе цену закупки
                decimal salePrice = product.SellingPrice > 0
                    ? product.SellingPrice
                    : product.PurchasePrice;
                db.ShipmentItems.Add(new ShipmentItem
                {
                    ShipmentId = shipment.Id,
                    ProductId  = row.ProductId,
                    Quantity   = row.Quantity,
                    Price      = salePrice
                });
                product.Stock -= row.Quantity;
            }

            db.SaveChanges();
            tx.Commit();

            _log.Info("Отгрузка {0} оформлена: {1}, получатель: {2}, позиций: {3}",
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
