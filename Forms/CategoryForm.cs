using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GreenStock.Data;
using GreenStock.Models;

namespace GreenStock.Forms
{
    public class CategoryForm : Form
    {
        private Label    lblListTitle, lblInputTitle;
        private ListBox  lstCategories;
        private TextBox  txtName;
        private Label    lblError;
        private Button   btnAdd, btnRename, btnDelete;

        public CategoryForm()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void InitializeComponent()
        {
            this.Text            = "Управление категориями";
            this.Size            = new Size(620, 480);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = Color.FromArgb(240, 240, 245);

            // ── Left: List ────────────────────────────────────
            lblListTitle = new Label { Text = "Список категорий", Font = new Font("Segoe UI", 10), Location = new Point(20, 60), AutoSize = true };

            lstCategories = new ListBox
            {
                Font          = new Font("Segoe UI", 10),
                Location      = new Point(20, 85),
                Size          = new Size(270, 250),
                BackColor     = Color.White,
                BorderStyle   = BorderStyle.FixedSingle
            };
            lstCategories.SelectedIndexChanged += (s, e) =>
            {
                if (lstCategories.SelectedItem != null)
                    txtName.Text = lstCategories.SelectedItem.ToString();
            };

            // ── Right: Input + Buttons ────────────────────────
            lblInputTitle = new Label { Text = "Название категории", Font = new Font("Segoe UI", 10), Location = new Point(330, 60), AutoSize = true };

            txtName = new TextBox
            {
                Font        = new Font("Segoe UI", 10),
                Location    = new Point(330, 85),
                Size        = new Size(240, 26),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblError = new Label { Text = "Категория уже существует", Font = new Font("Segoe UI", 8), ForeColor = Color.Red, Location = new Point(330, 114), AutoSize = true, Visible = false };

            btnAdd = new Button
            {
                Text      = "+ Добавить",
                Font      = new Font("Segoe UI", 10),
                Location  = new Point(330, 140),
                Size      = new Size(200, 34),
                BackColor = Color.FromArgb(100, 140, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            btnRename = new Button
            {
                Text      = "Переименовать",
                Font      = new Font("Segoe UI", 10),
                Location  = new Point(330, 185),
                Size      = new Size(200, 34),
                BackColor = Color.FromArgb(150, 170, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnRename.FlatAppearance.BorderSize = 0;
            btnRename.Click += BtnRename_Click;

            btnDelete = new Button
            {
                Text      = "✕  Удалить",
                Font      = new Font("Segoe UI", 10),
                Location  = new Point(330, 230),
                Size      = new Size(200, 34),
                BackColor = Color.FromArgb(200, 80, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;

            this.Controls.AddRange(new Control[]
            {
                lblListTitle, lstCategories,
                lblInputTitle, txtName, lblError,
                btnAdd, btnRename, btnDelete
            });
        }

        private void LoadCategories()
        {
            using var db = new AppDbContext();
            var cats = db.Categories.OrderBy(c => c.Name).ToList();
            lstCategories.Items.Clear();
            foreach (var c in cats) lstCategories.Items.Add(c.Name);
        }

        private int? GetSelectedId()
        {
            if (lstCategories.SelectedItem == null) return null;
            using var db = new AppDbContext();
            var cat = db.Categories.FirstOrDefault(c => c.Name == lstCategories.SelectedItem.ToString());
            return cat?.Id;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            lblError.Visible = false;
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;

            using var db = new AppDbContext();
            if (db.Categories.Any(c => c.Name == name))
            { lblError.Text = "Категория уже существует"; lblError.Visible = true; return; }

            db.Categories.Add(new Category { Name = name });
            db.SaveChanges();
            txtName.Clear();
            LoadCategories();
        }

        private void BtnRename_Click(object? sender, EventArgs e)
        {
            lblError.Visible = false;
            int? id = GetSelectedId();
            if (id == null) return;
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;

            using var db = new AppDbContext();
            if (db.Categories.Any(c => c.Name == name && c.Id != id))
            { lblError.Text = "Категория уже существует"; lblError.Visible = true; return; }

            var cat = db.Categories.Find(id);
            if (cat == null) return;
            cat.Name = name;
            db.SaveChanges();
            LoadCategories();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            lblError.Visible = false;
            int? id = GetSelectedId();
            if (id == null) return;
            string name = lstCategories.SelectedItem?.ToString() ?? "";

            var dlg = new DeleteConfirmForm($"категорию «{name}»");
            if (dlg.ShowDialog() != DialogResult.Yes) return;

            using var db = new AppDbContext();
            var cat = db.Categories.Find(id);
            if (cat == null) return;
            db.Categories.Remove(cat);
            try { db.SaveChanges(); LoadCategories(); }
            catch { lblError.Text = "Нельзя удалить: есть товары в этой категории"; lblError.Visible = true; }
        }
    }
}
