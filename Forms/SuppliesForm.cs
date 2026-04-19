using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using GreenStock.Data;
using GreenStock.Services;
using Npgsql;

namespace GreenStock;

/// <summary>
/// Форма для управления поставками товаров.
/// Позволяет вводить поставки вручную и импортировать из файлов.
/// </summary>
public class SuppliesForm : Form
{
    private Label _lblProduct = null!;
    private ComboBox _cmbProduct = null!;
    private Label _lblQty = null!;
    private NumericUpDown _nudQty = null!;
    private Label _lblPrice = null!;
    private NumericUpDown _nudPrice = null!;
    private Label _lblExpiry = null!;
    private DateTimePicker _dtpExpiry = null!;
    private Button _btnAddManual = null!;
    private Button _btnCalculate = null!;
    private Button _btnImport = null!;
    private Button _btnSave = null!;
    private Button _btnDelete = null!;
    private DataGridView _dgvSupplies = null!;
    private Label _lblTotal = null!;
    private Label _lblTitle = null!;

    private readonly string _connStr;
    private readonly Guid _userId;
    // ProductId is UUID in database -> use Guid here
    private List<(Guid ProductId, string ProductName, int Qty, decimal Price, DateTime Expiry)> _supplies = new();

    public SuppliesForm(string connStr, Guid userId)
    {
        _connStr = connStr;
        _userId = userId;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Поставка";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(950, 500);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        AutoScroll = false;

        // Layout: Left panel (240px) | Right panel (690px)
        const int leftW = 220;
        const int rightW = 690;
        const int leftX = 12;
        const int rightX = 244;
        const int topPadding = 12;

        // ── ЛЕВАЯ ПАНЕЛЬ: ВВОД ДАННЫХ ──────────────────────────────
        _lblProduct = new Label 
        { 
            Text = "Товар:", 
            Location = new Point(leftX, topPadding), 
            AutoSize = true,
            Font = new Font("Segoe UI", 9)
        };
        _cmbProduct = new ComboBox
        {
            Location = new Point(leftX, topPadding + 18),
            Size = new Size(leftW, 22),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9)
        };

        _lblQty = new Label 
        { 
            Text = "Количество:", 
            Location = new Point(leftX, topPadding + 52), 
            AutoSize = true,
            Font = new Font("Segoe UI", 9)
        };
        _nudQty = new NumericUpDown
        {
            Location = new Point(leftX, topPadding + 70),
            Size = new Size(100, 22),
            Minimum = 1,
            Maximum = 99999,
            Value = 1,
            Font = new Font("Segoe UI", 9)
        };

        _lblPrice = new Label 
        { 
            Text = "Цена закупки:", 
            Location = new Point(leftX, topPadding + 100), 
            AutoSize = true,
            Font = new Font("Segoe UI", 9)
        };
        _nudPrice = new NumericUpDown
        {
            Location = new Point(leftX, topPadding + 118),
            Size = new Size(100, 22),
            Minimum = 0,
            Maximum = 999999,
            DecimalPlaces = 2,
            Value = 0,
            Font = new Font("Segoe UI", 9)
        };

        _lblExpiry = new Label 
        { 
            Text = "Срок годности:", 
            Location = new Point(leftX, topPadding + 148), 
            AutoSize = true,
            Font = new Font("Segoe UI", 9)
        };
        _dtpExpiry = new DateTimePicker
        {
            Location = new Point(leftX, topPadding + 166),
            Size = new Size(120, 22),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now.AddMonths(12),
            Font = new Font("Segoe UI", 9)
        };

        // ── Кнопки в левой панели ───────────────
        _btnAddManual = new Button
        {
            Text = "Добавить в поставку",
            Location = new Point(leftX, topPadding + 200),
            Size = new Size(leftW, 28),
            BackColor = Color.FromArgb(70, 130, 180),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8, FontStyle.Bold)
        };
        _btnAddManual.FlatAppearance.BorderSize = 0;
        _btnAddManual.Click += BtnAddManual_Click;

        _btnCalculate = new Button
        {
            Text = "Очистить",
            Location = new Point(leftX, topPadding + 236),
            Size = new Size(leftW, 28),
            BackColor = Color.FromArgb(128, 128, 128),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 8, FontStyle.Bold)
        };
        _btnCalculate.FlatAppearance.BorderSize = 0;
        _btnCalculate.Click += (s, e) => { _supplies.Clear(); RefreshGrid(); _nudQty.Value = 1; _nudPrice.Value = 0; };

        // ── ПРАВАЯ ПАНЕЛЬ: ТАБЛИЦА И ИТОГИ ────────────────────────────
        _lblTitle = new Label
        {
            Text = "Товары в поставке:",
            Location = new Point(rightX, topPadding),
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        _dgvSupplies = new DataGridView
        {
            Location = new Point(rightX, topPadding + 22),
            Size = new Size(rightW, 210),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            Font = new Font("Segoe UI", 8),
            RowHeadersVisible = false,
            GridColor = Color.LightGray,
            ScrollBars = ScrollBars.Vertical,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        };
        _dgvSupplies.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(70, 130, 180);
        _dgvSupplies.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _dgvSupplies.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8, FontStyle.Bold);
        _dgvSupplies.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _dgvSupplies.EnableHeadersVisualStyles = false;
        _dgvSupplies.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        _dgvSupplies.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        _dgvSupplies.AllowUserToResizeRows = false;

        // ── Инициализация колонок таблицы ──────────────
        _dgvSupplies.Columns.Add("Товар", "Товар");
        _dgvSupplies.Columns.Add("Количество", "Количество");
        _dgvSupplies.Columns.Add("Цена", "Цена");
        _dgvSupplies.Columns.Add("СуммаПозиции", "Сумма позиции");

        // Установка ширины колонок
        _dgvSupplies.Columns["Товар"].Width = 280;
        _dgvSupplies.Columns["Количество"].Width = 80;
        _dgvSupplies.Columns["Цена"].Width = 100;
        _dgvSupplies.Columns["СуммаПозиции"].Width = 120;

        // ── Итоги ─────────────────────────
        _lblTotal = new Label
        {
            Text = "Итого позиций: 0   Общее количество: 0",
            Location = new Point(rightX, topPadding + 238),
            Size = new Size(rightW, 24),
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 8),
            BorderStyle = BorderStyle.FixedSingle,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(4, 0, 0, 0)
        };

        // ── НИЖНЯЯ ПАНЕЛЬ: КНОПКИ ──────────────────────────────────
        _btnImport = new Button
        {
            Text = "Импортировать из файла",
            Location = new Point(leftX, 430),
            Size = new Size(leftW, 32),
            BackColor = Color.FromArgb(128, 128, 128),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        _btnImport.FlatAppearance.BorderSize = 0;
        _btnImport.Click += BtnImport_Click;

        _btnSave = new Button
        {
            Text = "Сохранить поставку",
            Location = new Point(rightX, 430),
            Size = new Size(300, 32),
            BackColor = Color.FromArgb(70, 130, 180),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Click += BtnSave_Click;

        _btnDelete = new Button
        {
            Text = "Отменить",
            Location = new Point(rightX + 310, 430),
            Size = new Size(rightW - 310, 32),
            BackColor = Color.FromArgb(192, 57, 43),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        _btnDelete.FlatAppearance.BorderSize = 0;
        _btnDelete.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

        Controls.AddRange(new Control[]
        {
            _lblProduct, _cmbProduct, _lblQty, _nudQty, _lblPrice, _nudPrice,
            _lblExpiry, _dtpExpiry, _btnAddManual, _btnCalculate,
            _lblTitle, _dgvSupplies, _lblTotal, 
            _btnImport, _btnSave, _btnDelete
        });

        Load += (s, e) => LoadProducts();
    }

    private void LoadProducts()
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        var da = new NpgsqlDataAdapter("SELECT id, name FROM products ORDER BY name", conn);
        var dt = new DataTable();
        da.Fill(dt);
        _cmbProduct.DisplayMember = "name";
        _cmbProduct.ValueMember = "id";
        _cmbProduct.DataSource = dt;
    }

    private void BtnAddManual_Click(object? sender, EventArgs e)
    {
        // SelectedValue can be Guid or string (depending on DataSource), handle both
        object? raw = _cmbProduct.SelectedValue;
        Guid productId;
        if (raw is Guid g) productId = g;
        else if (raw is string s && Guid.TryParse(s, out g)) productId = g;
        else return;
        var productName = _cmbProduct.Text;
        int qty = (int)_nudQty.Value;
        decimal price = _nudPrice.Value;
        DateTime expiry = _dtpExpiry.Value.Date;

        if (qty <= 0 || price <= 0)
        {
            MessageBox.Show("Проверьте количество и цену.", Strings.Warning);
            return;
        }

        _supplies.Add((productId, productName, qty, price, expiry));
        RefreshGrid();

        _nudQty.Value = 1;
        _nudPrice.Value = 0;
        _dtpExpiry.Value = DateTime.Now.AddMonths(12);
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_dgvSupplies.CurrentRow?.Index is int idx && idx >= 0 && idx < _supplies.Count)
        {
            _supplies.RemoveAt(idx);
            RefreshGrid();
        }
    }

    private void BtnImport_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog { Filter = "CSV (*.csv)|*.csv|All files (*.*)|*.*" };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        try
        {
            var lines = File.ReadAllLines(ofd.FileName);
            int imported = 0;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                // Skip header line
                if (line.StartsWith("productId") || line.StartsWith("article")) continue;

                var parts = line.Split(';');
                if (parts.Length < 4) continue;

                string productIdOrArticle = parts[0].Trim();
                Guid productId = Guid.Empty;

                // Try parse as UUID first
                if (Guid.TryParse(productIdOrArticle, out Guid parsedGuid))
                {
                    productId = parsedGuid;
                }
                else
                {
                    // Try find by article name
                    try
                    {
                        using var conn2 = new NpgsqlConnection(_connStr);
                        conn2.Open();
                        using var cmd = new NpgsqlCommand("SELECT id FROM products WHERE article = @article LIMIT 1", conn2);
                        cmd.Parameters.AddWithValue("@article", productIdOrArticle);
                        var o = cmd.ExecuteScalar();
                        if (o != null && Guid.TryParse(o.ToString()!, out Guid foundId))
                            productId = foundId;
                    }
                    catch { }
                }

                if (productId == Guid.Empty) continue;

                if (!int.TryParse(parts[1].Trim(), out int qty)) continue;
                if (!decimal.TryParse(parts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture, out decimal price)) continue;
                if (!DateTime.TryParse(parts[3].Trim(), System.Globalization.CultureInfo.InvariantCulture, out DateTime expiry)) continue;

                // Try to get product name from DB; fallback to ID string if not found
                string productName = $"Product {productId}";
                try
                {
                    using var conn2 = new NpgsqlConnection(_connStr);
                    conn2.Open();
                    using var cmd = new NpgsqlCommand("SELECT name FROM products WHERE id = @id", conn2);
                    cmd.Parameters.AddWithValue("@id", productId);
                    var o = cmd.ExecuteScalar();
                    if (o != null) productName = o.ToString()!;
                }
                catch { /* ignore lookup errors, use fallback name */ }

                _supplies.Add((productId, productName, qty, price, expiry.Date));
                imported++;
            }

            RefreshGrid();
            MessageBox.Show($"Импортировано позиций: {imported}", Strings.Done);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка импорта: {ex.Message}", Strings.Error);
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_supplies.Count == 0)
        {
            MessageBox.Show("Добавьте хотя бы одну позицию.", Strings.Warning);
            return;
        }

        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            foreach (var (productId, _, qty, price, expiry) in _supplies)
            {
                // productId is Guid, Npgsql will infer UUID parameter type from Guid value
                // Update products table: increment stock, set purchase_price, and update expiry_date
                var cmdUpdate = new NpgsqlCommand(
                    "UPDATE products SET stock = stock + @qty, purchase_price = @price, expiry_date = @exp WHERE id = @pid", conn, tx);
                cmdUpdate.Parameters.AddWithValue("@qty", qty);
                cmdUpdate.Parameters.AddWithValue("@price", price);
                cmdUpdate.Parameters.AddWithValue("@exp", expiry);
                cmdUpdate.Parameters.AddWithValue("@pid", productId);
                cmdUpdate.ExecuteNonQuery();
            }

            tx.Commit();
            MessageBox.Show("Поставка успешно сохранена!", Strings.Done);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            MessageBox.Show($"Ошибка: {ex.Message}", Strings.Error);
        }
    }

    private void RefreshGrid()
    {
        _dgvSupplies.Rows.Clear();

        int totalQty = 0;
        decimal totalPrice = 0;

        foreach (var (_, name, qty, price, expiry) in _supplies)
        {
            string formattedPrice = CurrencyService.Format(price);
            decimal itemSum = qty * price;
            string formattedSum = CurrencyService.Format(itemSum);

            _dgvSupplies.Rows.Add(name, qty, formattedPrice, formattedSum);
            totalQty += qty;
            totalPrice += itemSum;
        }

        string totalFormatted = CurrencyService.Format(totalPrice);
        _lblTotal.Text = $"{Strings.Supplies_TotalLabel} {totalFormatted} | {Strings.Supplies_TotalQty} {totalQty} шт.";
    }
}
