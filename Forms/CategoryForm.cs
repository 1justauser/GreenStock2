using GreenStock.Data;
using GreenStock.Logging;
using GreenStock.Models;
using NLog;

namespace GreenStock.Forms;

/// <summary>
/// Форма управления категориями товаров.
/// Позволяет добавлять, переименовывать и удалять категории.
/// </summary>
public class CategoryForm : Form
{
    private static readonly ILogger _log = AppLogger.For<CategoryForm>();

    private Label   _lblListTitle  = null!;
    private Label   _lblInputTitle = null!;
    private ListBox _lstCategories = null!;
    private TextBox _txtName       = null!;
    private Label   _lblError      = null!;
    private Button  _btnAdd        = null!;
    private Button  _btnRename     = null!;
    private Button  _btnDelete     = null!;

    /// <summary>
    /// Инициализирует форму и загружает список категорий из БД.
    /// </summary>
    public CategoryForm()
    {
        InitializeComponent();
        LoadCategories();
    }

    private void InitializeComponent()
    {
        Text            = Strings.Category_Title;
        Size            = new Size(620, 480);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = Color.FromArgb(240, 240, 245);

        _lblListTitle = new Label
            { Text = Strings.Category_ListTitle, Font = new Font("Segoe UI", 10), Location = new Point(20, 60), AutoSize = true };

        _lstCategories = new ListBox
        {
            Font        = new Font("Segoe UI", 10),
            Location    = new Point(20, 85),
            Size        = new Size(270, 250),
            BackColor   = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        _lstCategories.SelectedIndexChanged += (s, e) =>
        {
            if (_lstCategories.SelectedItem != null)
                _txtName.Text = _lstCategories.SelectedItem.ToString();
        };

        _lblInputTitle = new Label
            { Text = Strings.Category_InputTitle, Font = new Font("Segoe UI", 10), Location = new Point(330, 60), AutoSize = true };

        _txtName = new TextBox
        {
            Font        = new Font("Segoe UI", 10),
            Location    = new Point(330, 85),
            Size        = new Size(240, 26),
            BorderStyle = BorderStyle.FixedSingle
        };

        _lblError = new Label
        {
            Text     = Strings.Category_ErrAlreadyExists,
            Font     = new Font("Segoe UI", 8),
            ForeColor = Color.Red,
            Location  = new Point(330, 114),
            AutoSize  = true,
            Visible   = false
        };

        _btnAdd = new Button
        {
            Text      = Strings.Category_BtnAdd,
            Font      = new Font("Segoe UI", 10),
            Location  = new Point(330, 140),
            Size      = new Size(200, 34),
            BackColor = Color.FromArgb(100, 140, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnAdd.FlatAppearance.BorderSize = 0;
        _btnAdd.Click += BtnAdd_Click;

        _btnRename = new Button
        {
            Text      = Strings.Category_BtnRename,
            Font      = new Font("Segoe UI", 10),
            Location  = new Point(330, 185),
            Size      = new Size(200, 34),
            BackColor = Color.FromArgb(150, 170, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnRename.FlatAppearance.BorderSize = 0;
        _btnRename.Click += BtnRename_Click;

        _btnDelete = new Button
        {
            Text      = Strings.Category_BtnDelete,
            Font      = new Font("Segoe UI", 10),
            Location  = new Point(330, 230),
            Size      = new Size(200, 34),
            BackColor = Color.FromArgb(200, 80, 80),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnDelete.FlatAppearance.BorderSize = 0;
        _btnDelete.Click += BtnDelete_Click;

        Controls.AddRange(new Control[]
        {
            _lblListTitle, _lstCategories,
            _lblInputTitle, _txtName, _lblError,
            _btnAdd, _btnRename, _btnDelete
        });
    }

    private void LoadCategories()
    {
        using var db = new AppDbContext();
        var cats = db.Categories.OrderBy(c => c.Name).ToList();
        _lstCategories.Items.Clear();
        foreach (var c in cats) _lstCategories.Items.Add(c.Name);
    }

    /// <summary>
    /// Возвращает Guid выбранной категории или <c>null</c>, если ничего не выбрано.
    /// </summary>
    private Guid? GetSelectedId()
    {
        if (_lstCategories.SelectedItem == null) return null;
        using var db = new AppDbContext();
        var cat = db.Categories.FirstOrDefault(c => c.Name == _lstCategories.SelectedItem.ToString());
        return cat?.Id;
    }

    private void BtnAdd_Click(object? sender, EventArgs e)
    {
        _lblError.Visible = false;
        var name = _txtName.Text.Trim();
        if (string.IsNullOrEmpty(name)) return;

        using var db = new AppDbContext();
        if (db.Categories.Any(c => c.Name == name))
        {
            _lblError.Text    = Strings.Category_ErrAlreadyExists;
            _lblError.Visible = true;
            return;
        }

        db.Categories.Add(new Category { Name = name });
        db.SaveChanges();
        _log.Info("Добавлена категория: {0}", name);
        _txtName.Clear();
        LoadCategories();
    }

    private void BtnRename_Click(object? sender, EventArgs e)
    {
        _lblError.Visible = false;
        var id   = GetSelectedId();
        if (id == null) return;
        var name = _txtName.Text.Trim();
        if (string.IsNullOrEmpty(name)) return;

        using var db = new AppDbContext();
        if (db.Categories.Any(c => c.Name == name && c.Id != id))
        {
            _lblError.Text    = Strings.Category_ErrAlreadyExists;
            _lblError.Visible = true;
            return;
        }

        var cat = db.Categories.Find(id);
        if (cat == null) return;
        var oldName = cat.Name;
        cat.Name = name;
        db.SaveChanges();
        _log.Info("Категория переименована: {0} → {1}", oldName, name);
        LoadCategories();
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        _lblError.Visible = false;
        var id   = GetSelectedId();
        if (id == null) return;
        var name = _lstCategories.SelectedItem?.ToString() ?? string.Empty;

        var dlg = new DeleteConfirmForm(Strings.Category_DeleteConfirm(name));
        if (dlg.ShowDialog() != DialogResult.Yes) return;

        using var db = new AppDbContext();
        var cat = db.Categories.Find(id);
        if (cat == null) return;
        db.Categories.Remove(cat);
        try
        {
            db.SaveChanges();
            _log.Info("Удалена категория: {0}", name);
            LoadCategories();
        }
        catch
        {
            _log.Warn("Попытка удалить категорию {0} с привязанными товарами", name);
            _lblError.Text    = Strings.Category_ErrHasProducts;
            _lblError.Visible = true;
        }
    }
}
