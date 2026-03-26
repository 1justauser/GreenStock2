using GreenStock.Data;
using GreenStock.Logging;
using GreenStock.Resources;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GreenStock.Forms;

/// <summary>
/// Форма просмотра истории отгрузок.
/// Показывает список всех накладных и состав выбранной.
/// Доступна только администратору.
/// </summary>
public class HistoryForm : Form
{
    private static readonly ILogger _log = AppLogger.For<HistoryForm>();

    private Label        _lblShipments = null!;
    private Label        _lblItems     = null!;
    private Panel        _sepShipments = null!;
    private Panel        _sepItems     = null!;
    private DataGridView _dgvShipments = null!;
    private DataGridView _dgvItems     = null!;

    /// <summary>
    /// Инициализирует форму и загружает историю отгрузок.
    /// </summary>
    public HistoryForm()
    {
        InitializeComponent();
        LoadShipments();
    }

    private void InitializeComponent()
    {
        Text            = Strings.History_Title;
        Size            = new Size(750, 500);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        BackColor       = Color.FromArgb(240, 240, 245);
        MinimumSize     = new Size(750, 500);

        _lblShipments = new Label
            { Text = Strings.History_LabelShipments, Font = new Font("Segoe UI", 10), Location = new Point(10, 10), AutoSize = true };
        _sepShipments = new Panel
            { Location = new Point(10, 30), Size = new Size(715, 1), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.Silver };

        _dgvShipments = new DataGridView
        {
            Location              = new Point(10, 35),
            Size                  = new Size(715, 160),
            Anchor                = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly              = true,
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect           = false,
            BackgroundColor       = Color.White,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            Font                  = new Font("Segoe UI", 9)
        };
        _dgvShipments.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(220, 230, 240);
        _dgvShipments.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
        _dgvShipments.EnableHeadersVisualStyles = false;
        _dgvShipments.SelectionChanged         += DgvShipments_SelectionChanged;

        _lblItems = new Label
            { Text = Strings.History_LabelItems("—"), Font = new Font("Segoe UI", 10), Location = new Point(10, 208), AutoSize = true };
        _sepItems = new Panel
            { Location = new Point(10, 228), Size = new Size(715, 1), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.Silver };

        _dgvItems = new DataGridView
        {
            Location              = new Point(10, 233),
            Size                  = new Size(715, 170),
            Anchor                = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly              = true,
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            BackgroundColor       = Color.White,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            Font                  = new Font("Segoe UI", 9)
        };
        _dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(220, 230, 240);
        _dgvItems.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
        _dgvItems.EnableHeadersVisualStyles = false;

        Controls.AddRange(new Control[]
        {
            _lblShipments, _sepShipments, _dgvShipments,
            _lblItems, _sepItems, _dgvItems
        });
    }

    private void LoadShipments()
    {
        try
        {
            using var db = new AppDbContext();
            var shipments = db.Shipments
                .Include(s => s.CreatedByUser)   // правильное имя навигационного свойства
                .Include(s => s.Items)            // правильное имя коллекции позиций
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            _log.Debug("История: загружено {0} накладных", shipments.Count);

            _dgvShipments.DataSource = shipments.Select((s, idx) => new
            {
                N           = idx + 1,
                Дата_Время  = s.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
                Кто_оформил = s.CreatedByUser.Login,
                Получатель  = s.Recipient,
                Позиций     = s.Items.Count,
                _Id         = s.Id   // Guid
            }).ToList();

            if (_dgvShipments.Columns.Contains("_Id"))         _dgvShipments.Columns["_Id"]!.Visible           = false;
            if (_dgvShipments.Columns.Contains("Дата_Время"))  _dgvShipments.Columns["Дата_Время"]!.HeaderText  = Strings.History_ColDate;
            if (_dgvShipments.Columns.Contains("Кто_оформил")) _dgvShipments.Columns["Кто_оформил"]!.HeaderText = Strings.History_ColWho;
            if (_dgvShipments.Columns.Contains("Получатель"))  _dgvShipments.Columns["Получатель"]!.HeaderText  = Strings.History_ColRecipient;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Ошибка загрузки истории отгрузок");
            MessageBox.Show($"{Strings.Error}:\n{ex.Message}",
                Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvShipments_SelectionChanged(object? sender, EventArgs e)
    {
        if (_dgvShipments.CurrentRow == null) return;

        // ID теперь Guid
        if (_dgvShipments.CurrentRow.Cells["_Id"].Value is not Guid shipmentId) return;
        _lblItems.Text = Strings.History_LabelItems(shipmentId.ToString()[..8] + "…");

        using var db = new AppDbContext();
        var items = db.ShipmentItems
            .Include(i => i.Product)
            .Where(i => i.ShipmentId == shipmentId)
            .ToList();

        _dgvItems.DataSource = items.Select(i => new
        {
            Товар      = i.Product.Name,
            Артикул    = i.Product.Article,
            Ед_изм     = i.Product.Unit,
            Количество = i.Quantity
        }).ToList();

        if (_dgvItems.Columns.Contains("Ед_изм"))
            _dgvItems.Columns["Ед_изм"]!.HeaderText = Strings.Catalog_ColUnit;
    }
}
