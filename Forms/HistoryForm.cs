using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GreenStock.Data;
using Microsoft.EntityFrameworkCore;

namespace GreenStock.Forms
{
    public class HistoryForm : Form
    {
        private Label        lblShipments, lblItems;
        private Panel        sepShipments, sepItems;
        private DataGridView dgvShipments, dgvItems;

        public HistoryForm()
        {
            InitializeComponent();
            LoadShipments();
        }

        private void InitializeComponent()
        {
            this.Text            = "История отгрузок";
            this.Size            = new Size(750, 500);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor       = Color.FromArgb(240, 240, 245);
            this.MinimumSize     = new Size(750, 500);

            // ── Накладные label + separator ───────────────────
            lblShipments = new Label { Text = "Накладные", Font = new Font("Segoe UI", 10), Location = new Point(10, 10), AutoSize = true };
            sepShipments = new Panel { Location = new Point(10, 30), Size = new Size(715, 1), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.Silver };

            dgvShipments = new DataGridView
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
            dgvShipments.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(220, 230, 240);
            dgvShipments.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvShipments.EnableHeadersVisualStyles = false;
            dgvShipments.SelectionChanged         += DgvShipments_SelectionChanged;

            // ── Состав накладной label + separator ────────────
            lblItems = new Label { Text = "Состав накладной №1", Font = new Font("Segoe UI", 10), Location = new Point(10, 208), AutoSize = true };
            sepItems = new Panel { Location = new Point(10, 228), Size = new Size(715, 1), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.Silver };

            dgvItems = new DataGridView
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
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(220, 230, 240);
            dgvItems.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvItems.EnableHeadersVisualStyles = false;

            this.Controls.AddRange(new Control[]
            {
                lblShipments, sepShipments, dgvShipments,
                lblItems, sepItems, dgvItems
            });
        }

        private void LoadShipments()
        {
            using var db = new AppDbContext();
            var shipments = db.Shipments
                .Include(s => s.User)
                .Include(s => s.ShipmentItems)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            dgvShipments.DataSource = shipments.Select(s => new
            {
                N           = s.Id,
                Дата_Время  = s.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
                Кто_оформил = s.User.Login,
                Получатель  = s.Recipient,
                Позиций     = s.ShipmentItems.Count,
                _Id         = s.Id
            }).ToList();

            if (dgvShipments.Columns.Contains("_Id"))         dgvShipments.Columns["_Id"]!.Visible = false;
            if (dgvShipments.Columns.Contains("Дата_Время"))  dgvShipments.Columns["Дата_Время"]!.HeaderText  = "Дата и Время";
            if (dgvShipments.Columns.Contains("Кто_оформил")) dgvShipments.Columns["Кто_оформил"]!.HeaderText = "Кто оформил";
        }

        private void DgvShipments_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvShipments.CurrentRow == null) return;
            int shipmentId = (int)dgvShipments.CurrentRow.Cells["_Id"].Value;
            lblItems.Text  = $"Состав накладной №{shipmentId}";

            using var db = new AppDbContext();
            var items = db.ShipmentItems
                .Include(i => i.Product)
                .Where(i => i.ShipmentId == shipmentId)
                .ToList();

            dgvItems.DataSource = items.Select(i => new
            {
                Товар      = i.Product.Name,
                Артикул    = i.Product.Article,
                Ед_изм     = i.Product.Unit,
                Количество = i.Quantity
            }).ToList();

            if (dgvItems.Columns.Contains("Ед_изм")) dgvItems.Columns["Ед_изм"]!.HeaderText = "Ед. изм.";
        }
    }
}
