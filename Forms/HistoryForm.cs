using GreenStock.Data;
using GreenStock.Logging;
using GreenStock.Services;
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
            { Text = Strings.History_LabelItems(0), Font = new Font("Segoe UI", 10), Location = new Point(10, 208), AutoSize = true };
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
            var shipments = HistoryService.GetAllShipments();

            _log.Debug("История: загружено {0} накладных", shipments.Count);

            _dgvShipments.DataSource = shipments.Select((s, idx) => new
            {
                N           = idx + 1,
                Дата_Время  = s.CreatedAt,
                Кто_оформил = s.CreatedBy,
                Получатель  = s.Recipient,
                Позиций     = s.ItemCount,
                Сумма       = $"{s.TotalAmount:N2} ₽",
                _Id         = s.Id   // Guid
            }).ToList();

            if (_dgvShipments.Columns.Contains("_Id"))         _dgvShipments.Columns["_Id"]!.Visible           = false;
            if (_dgvShipments.Columns.Contains("Дата_Время"))  _dgvShipments.Columns["Дата_Время"]!.HeaderText  = Strings.History_ColDate;
            if (_dgvShipments.Columns.Contains("Кто_оформил")) _dgvShipments.Columns["Кто_оформил"]!.HeaderText = Strings.History_ColWho;
            if (_dgvShipments.Columns.Contains("Получатель"))  _dgvShipments.Columns["Получатель"]!.HeaderText  = Strings.History_ColRecipient;
            if (_dgvShipments.Columns.Contains("Сумма"))       _dgvShipments.Columns["Сумма"]!.HeaderText       = "Сумма";
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
        var shipmentNumber = _dgvShipments.CurrentRow.Index + 1;
        _lblItems.Text = Strings.History_LabelItems(shipmentNumber);

        try
        {
            var items = HistoryService.GetShipmentItems(shipmentId);

            _dgvItems.DataSource = items.Select(i => new
            {
                Товар      = i.ProductName,
                Артикул    = i.Article,
                Ед_изм     = i.Unit,
                Количество = i.Quantity,
                Цена       = $"{i.Price:N2} ₽",
                Сумма      = $"{i.Total:N2} ₽"
            }).ToList();

            if (_dgvItems.Columns.Contains("Товар"))      _dgvItems.Columns["Товар"]!.HeaderText      = Strings.Catalog_ColName;
            if (_dgvItems.Columns.Contains("Артикул"))    _dgvItems.Columns["Артикул"]!.HeaderText    = Strings.Catalog_ColArticle;
            if (_dgvItems.Columns.Contains("Ед_изм"))     _dgvItems.Columns["Ед_изм"]!.HeaderText     = Strings.Catalog_ColUnit;
            if (_dgvItems.Columns.Contains("Цена"))       _dgvItems.Columns["Цена"]!.HeaderText       = "Цена";
            if (_dgvItems.Columns.Contains("Сумма"))      _dgvItems.Columns["Сумма"]!.HeaderText      = "Сумма";
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Ошибка загрузки позиций отгрузки");
            MessageBox.Show($"{Strings.Error}:\n{ex.Message}",
                Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
