using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GreenStock;
using ClosedXML.Excel;
using GreenStock.Services;
using Npgsql;

namespace GreenStock.Forms;

/// <summary>
/// Форма для формирования и экспорта отчетов по отгрузкам.
/// </summary>
public class ReportsForm : Form
{
    private DateTimePicker _dtpFrom = null!;
    private DateTimePicker _dtpTo = null!;
    private Button _btnGenerate = null!;
    private Button _btnExport = null!;
    private DataGridView _dgvReport = null!;
    private Label _lblTotal = null!;

    private readonly string _connStr;

    public ReportsForm(string connStr)
    {
        _connStr = connStr;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = Strings.Reports_Title;
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1000, 700);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10);

        // ── Фильтр даты ───────────────────
        var lblFrom = new Label 
        { 
            Text = Strings.Reports_LabelFrom, 
            Location = new Point(12, 12), 
            AutoSize = true,
            Font = new Font("Segoe UI", 9)
        };

        _dtpFrom = new DateTimePicker
        {
            Location = new Point(85, 9),
            Size = new Size(140, 28),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now.AddMonths(-1),
            Font = new Font("Segoe UI", 9)
        };

        var lblTo = new Label 
        { 
            Text = Strings.Reports_LabelTo, 
            Location = new Point(245, 12), 
            AutoSize = true,
            Font = new Font("Segoe UI", 9)
        };

        _dtpTo = new DateTimePicker
        {
            Location = new Point(290, 9),
            Size = new Size(140, 28),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Now,
            Font = new Font("Segoe UI", 9)
        };

        _btnGenerate = new Button
        {
            Text = Strings.Reports_BtnGenerate,
            Location = new Point(450, 8),
            Size = new Size(150, 32),
            BackColor = Color.FromArgb(40, 120, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        _btnGenerate.FlatAppearance.BorderSize = 0;
        _btnGenerate.Click += BtnGenerate_Click;

        // ── DataGridView ──────────────────
        _dgvReport = new DataGridView
        {
            Location = new Point(12, 55),
            Size = new Size(960, 520),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Segoe UI", 9),
            RowHeadersVisible = false
        };
        _dgvReport.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 42, 74);
        _dgvReport.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        _dgvReport.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _dgvReport.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _dgvReport.EnableHeadersVisualStyles = false;
        _dgvReport.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 250);

        // ── Итоги ─────────────────────────
        _lblTotal = new Label
        {
            Text = Strings.Reports_Total,
            Location = new Point(12, 585),
            Size = new Size(600, 30),
            ForeColor = Color.FromArgb(28, 42, 74),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BorderStyle = BorderStyle.None
        };

        _btnExport = new Button
        {
            Text = Strings.Reports_BtnExport,
            Location = new Point(835, 585),
            Size = new Size(137, 35),
            BackColor = Color.FromArgb(100, 150, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        _btnExport.FlatAppearance.BorderSize = 0;
        _btnExport.Click += BtnExport_Click;

        Controls.AddRange(new Control[]
        {
            lblFrom, _dtpFrom, lblTo, _dtpTo, _btnGenerate,
            _dgvReport, _lblTotal, _btnExport
        });
    }

    private void BtnGenerate_Click(object? sender, EventArgs e)
    {
        DateTime from = _dtpFrom.Value.Date;
        DateTime to = _dtpTo.Value.Date.AddDays(1);

        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();

        string sql = @"SELECT 
            SUBSTRING(s.id::text, 1, 8) AS ""№ отгрузки"", 
            s.created_at::date AS ""Дата"", 
            s.recipient AS ""Получатель"", 
            COALESCE(SUM(si.quantity * si.price), 0) AS ""Сумма"", 
            COALESCE(SUM(si.quantity * si.price), 0) - COALESCE(SUM(si.quantity * COALESCE(p.purchase_price, 0)), 0) AS ""Прибыль""
            FROM shipments s 
            LEFT JOIN shipment_items si ON si.shipment_id = s.id 
            LEFT JOIN products p ON p.id = si.product_id
            WHERE s.created_at >= @from AND s.created_at < @to 
            GROUP BY s.id, s.created_at, s.recipient 
            ORDER BY s.created_at DESC";

        var da = new NpgsqlDataAdapter(sql, conn);
        da.SelectCommand.Parameters.AddWithValue("@from", from);
        da.SelectCommand.Parameters.AddWithValue("@to", to);
        var dt = new DataTable();
        da.Fill(dt);
        _dgvReport.DataSource = dt;

        decimal totalSum = 0, totalProfit = 0;
        foreach (DataRow row in dt.Rows)
        {
            totalSum += row["Сумма"] != DBNull.Value ? (decimal)row["Сумма"] : 0;
            totalProfit += row["Прибыль"] != DBNull.Value ? (decimal)row["Прибыль"] : 0;
        }

        string totalText = $"{Strings.Reports_Total} Сумма = {CurrencyService.Format(totalSum)} | Прибыль = {CurrencyService.Format(totalProfit)}";
        _lblTotal.Text = totalText;
    }

    private void BtnExport_Click(object? sender, EventArgs e)
    {
        if (_dgvReport.DataSource == null)
        {
            MessageBox.Show("Сначала сформируйте отчёт.", Strings.Warning);
            return;
        }

        using var frm = new Form
        {
            Text = "Выбор формата экспорта",
            Size = new Size(350, 150),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White
        };

        var lblFormat = new Label
        {
            Text = "Выберите формат для экспорта:",
            Location = new Point(20, 20),
            AutoSize = true,
            Font = new Font("Segoe UI", 10)
        };

        var btnExcel = new Button
        {
            Text = "📊 Excel (XLSX)",
            Location = new Point(20, 60),
            Size = new Size(130, 40),
            BackColor = Color.FromArgb(0, 176, 80),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            DialogResult = DialogResult.Yes
        };
        btnExcel.FlatAppearance.BorderSize = 0;

        var btnCsv = new Button
        {
            Text = "📄 CSV",
            Location = new Point(200, 60),
            Size = new Size(130, 40),
            BackColor = Color.FromArgb(68, 114, 196),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            DialogResult = DialogResult.No
        };
        btnCsv.FlatAppearance.BorderSize = 0;

        frm.Controls.AddRange(new Control[] { lblFormat, btnExcel, btnCsv });
        frm.AcceptButton = btnExcel;
        frm.CancelButton = btnCsv;

        DialogResult result = frm.ShowDialog();

        if (result == DialogResult.Yes)
            ExportToExcel();
        else if (result == DialogResult.No)
            ExportToCsv();
    }

    private void ExportToExcel()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "Excel (*.xlsx)|*.xlsx",
            FileName = $"report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx"
        };

        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            var dt = (DataTable?)_dgvReport.DataSource;
            if (dt == null) return;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Отчёт");

            // ── Заголовки ──────────────────────
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = dt.Columns[i].ColumnName;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(28, 42, 74);
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // ── Данные ────────────────────────
            int rowNum = 2;
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var cell = worksheet.Cell(rowNum, i + 1);
                    cell.Value = row[i]?.ToString() ?? "";

                    // Форматирование числовых колонок
                    if (i >= 3) // Сумма и Прибыль
                    {
                        if (decimal.TryParse(row[i]?.ToString(), out var numValue))
                        {
                            cell.Value = numValue;
                            cell.Style.NumberFormat.Format = "#,##0.00";
                        }
                    }

                    // Чередующиеся цвета строк
                    if (rowNum % 2 == 0)
                        cell.Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 250);
                }
                rowNum++;
            }

            // ── Авто-ширина колонок ────────────
            worksheet.Columns().AdjustToContents();

            // ── Итоги ───────────────────────────
            int totalRow = rowNum + 1;
            worksheet.Cell(totalRow, 1).Value = "ИТОГО";
            worksheet.Cell(totalRow, 1).Style.Font.Bold = true;
            worksheet.Cell(totalRow, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(200, 200, 200);

            // Сумма формула
            if (dt.Columns.Count > 3)
            {
                var sumCell = worksheet.Cell(totalRow, 4);
                sumCell.FormulaA1 = $"SUM(D2:D{rowNum - 1})";
                sumCell.Style.Font.Bold = true;
                sumCell.Style.Fill.BackgroundColor = XLColor.FromArgb(200, 200, 200);
                sumCell.Style.NumberFormat.Format = "#,##0.00";
            }

            // Прибыль формула
            if (dt.Columns.Count > 4)
            {
                var profitCell = worksheet.Cell(totalRow, 5);
                profitCell.FormulaA1 = $"SUM(E2:E{rowNum - 1})";
                profitCell.Style.Font.Bold = true;
                profitCell.Style.Fill.BackgroundColor = XLColor.FromArgb(200, 200, 200);
                profitCell.Style.NumberFormat.Format = "#,##0.00";
            }

            workbook.SaveAs(sfd.FileName);
            MessageBox.Show($"Отчёт экспортирован в Excel!\n{sfd.FileName}", Strings.Done);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта в Excel: {ex.Message}", Strings.Error);
        }
    }

    private void ExportToCsv()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = $"report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv"
        };

        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            var dt = (DataTable?)_dgvReport.DataSource;
            if (dt == null) return;

            var sb = new StringBuilder();

            // ── Заголовки ──────────────────────
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sb.Append("\"" + dt.Columns[i].ColumnName.Replace("\"", "\"\"") + "\"");
                if (i < dt.Columns.Count - 1) sb.Append(";");
            }
            sb.AppendLine();

            // ── Данные ─────────────────────────
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string value = row[i]?.ToString() ?? "";
                    sb.Append("\"" + value.Replace("\"", "\"\"") + "\"");
                    if (i < dt.Columns.Count - 1) sb.Append(";");
                }
                sb.AppendLine();
            }

            // ── Итоги ───────────────────────────
            sb.AppendLine();
            sb.Append($"\"{Strings.Reports_Total}\"");
            for (int i = 1; i < dt.Columns.Count; i++) sb.Append(";");
            sb.AppendLine();

            File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"Отчёт экспортирован в CSV!\n{sfd.FileName}", Strings.Done);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта в CSV: {ex.Message}", Strings.Error);
        }
    }
}