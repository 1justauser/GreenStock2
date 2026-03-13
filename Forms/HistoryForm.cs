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
        private DataGridView dgvShipments, dgvItems;

        public HistoryForm()
        {
            InitializeComponent();
            LoadShipments();
        }

        private void InitializeComponent()
        {
            this.Text        = "История отгрузок";
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor   = Color.White;
            this.WindowState = FormWindowState.Maximized;
            this.Size        = new Size(1200, 700);

            lblShipments = new Label { Text = "Накладные", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(12, 10), AutoSize = true };

            dgvShipments = new DataGridView
            {
                Location              = new Point(12, 35),
                Size                  = new Size(1160, 250),
                Anchor                = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                BackgroundColor       = Color.White,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                Font                  = new Font("Segoe UI", 10)
            };
            dgvShipments.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 42, 74);
            dgvShipments.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvShipments.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvShipments.EnableHeadersVisualStyles = false;
            dgvShipments.SelectionChanged         += DgvShipments_SelectionChanged;

            lblItems = new Label { Text = "Состав накладной", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(12, 295), AutoSize = true };

            dgvItems = new DataGridView
            {
                Location              = new Point(12, 320),
                Size                  = new Size(1160, 300),
                Anchor                = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                BackgroundColor       = Color.White,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                Font                  = new Font("Segoe UI", 10)
            };
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 42, 74);
            dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvItems.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvItems.EnableHeadersVisualStyles = false;

            this.Controls.AddRange(new Control[] { lblShipments, dgvShipments, lblItems, dgvItems });
        }

        private void LoadShipments()
        {
            using var db = new AppDbContext();
            var shipments = db.Shipments.Include(s => s.User).Include(s => s.ShipmentItems).OrderByDescending(s => s.CreatedAt).ToList();

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
            if (dgvShipments.Columns.Contains("Дата_Время"))  dgvShipments.Columns["Дата_Время"]!.HeaderText = "Дата и Время";
            if (dgvShipments.Columns.Contains("Кто_оформил")) dgvShipments.Columns["Кто_оформил"]!.HeaderText = "Кто оформил";
        }

        private void DgvShipments_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvShipments.CurrentRow == null) return;
            int shipmentId = (int)dgvShipments.CurrentRow.Cells["_Id"].Value;
            lblItems.Text  = $"Состав накладной {shipmentId}";

            using var db = new AppDbContext();
            var items = db.ShipmentItems.Include(i => i.Product).Where(i => i.ShipmentId == shipmentId).ToList();

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
