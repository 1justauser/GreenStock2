using GreenStock.Data;
using GreenStock.Logging;
using GreenStock.Models;
using GreenStock.Resources;
using NLog;

namespace GreenStock.Forms;

/// <summary>
/// Форма добавления или редактирования товара.
/// </summary>
public class ProductForm : Form
{
    private static readonly ILogger _log = AppLogger.For<ProductForm>();

    private readonly Product? _existing;

    private Label         _lblArticle      = null!;
    private Label         _lblName         = null!;
    private Label         _lblCategory     = null!;
    private Label         _lblUnit         = null!;
    private Label         _lblPrice        = null!;
    private Label         _lblStock        = null!;
    private Label         _lblExpiry       = null!;
    private TextBox       _txtArticle      = null!;
    private TextBox       _txtName         = null!;
    private TextBox       _txtStock        = null!;
    private ComboBox      _cmbCategory     = null!;
    private ComboBox      _cmbUnit         = null!;
    private NumericUpDown _nudPrice        = null!;
    private DateTimePicker _dtpExpiry      = null!;
    private CheckBox      _chkNoExpiry     = null!;
    private Label         _lblArticleError = null!;
    private Label         _lblNameError    = null!;
    private Button        _btnSave         = null!;
    private Button        _btnCancel       = null!;

    /// <summary>
    /// Инициализирует форму.
    /// </summary>
    /// <param name="existing">Существующий товар для редактирования, или <c>null</c> для добавления.</param>
    public ProductForm(Product? existing)
    {
        _existing = existing;
        InitializeComponent();
        LoadCategories();
        if (_existing != null) FillFields();
    }

    private void InitializeComponent()
    {
        var isEdit = _existing != null;

        Text            = isEdit ? Strings.Product_TitleEdit : Strings.Product_TitleAdd;
        Size            = new Size(400, 470);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        BackColor       = Color.FromArgb(240, 240, 245);

        const int labelX = 30;
        const int inputX = 165;
        const int inputW = 185;
        const int rowH   = 50;
        const int startY = 20;

        Label MakeLabel(string text, int row) => new Label
        {
            Text     = text,
            Font     = new Font("Segoe UI", 10),
            Location = new Point(labelX, startY + row * rowH + 4),
            AutoSize = true
        };

        // ── Артикул ───────────────────────────────────────────
        _lblArticle = MakeLabel(Strings.Product_LabelArticle, 0);
        _txtArticle = new TextBox
        {
            Font      = new Font("Segoe UI", 10),
            Location  = new Point(inputX, startY + 0 * rowH),
            Size      = new Size(inputW, 24),
            ReadOnly  = isEdit,
            BackColor = isEdit ? Color.FromArgb(220, 220, 220) : Color.White
        };
        _lblArticleError = new Label
        {
            Text     = Strings.Product_ErrArticleExists,
            Font     = new Font("Segoe UI", 8),
            ForeColor = Color.Red,
            Location  = new Point(inputX, startY + 0 * rowH + 26),
            AutoSize  = true,
            Visible   = false
        };

        // ── Название ──────────────────────────────────────────
        _lblName = MakeLabel(Strings.Product_LabelName, 1);
        _txtName = new TextBox
            { Font = new Font("Segoe UI", 10), Location = new Point(inputX, startY + 1 * rowH), Size = new Size(inputW, 24) };
        _lblNameError = new Label
        {
            Text     = Strings.RequiredField,
            Font     = new Font("Segoe UI", 8),
            ForeColor = Color.Red,
            Location  = new Point(inputX, startY + 1 * rowH + 26),
            AutoSize  = true,
            Visible   = false
        };

        // ── Категория ─────────────────────────────────────────
        _lblCategory = MakeLabel(Strings.Product_LabelCategory, 2);
        _cmbCategory = new ComboBox
            { Font = new Font("Segoe UI", 10), Location = new Point(inputX, startY + 2 * rowH), Size = new Size(inputW, 24), DropDownStyle = ComboBoxStyle.DropDownList };

        // ── Единица измерения ─────────────────────────────────
        _lblUnit = MakeLabel(Strings.Product_LabelUnit, 3);
        _cmbUnit = new ComboBox
            { Font = new Font("Segoe UI", 10), Location = new Point(inputX, startY + 3 * rowH), Size = new Size(100, 24), DropDownStyle = ComboBoxStyle.DropDownList };
        _cmbUnit.Items.AddRange(new[] { "шт", "пак", "кг", "л", "г" });
        _cmbUnit.SelectedIndex = 0;

        // ── Цена закупки ──────────────────────────────────────
        _lblPrice = MakeLabel(Strings.Product_LabelPrice, 4);
        _nudPrice = new NumericUpDown
            { Font = new Font("Segoe UI", 10), Location = new Point(inputX, startY + 4 * rowH), Size = new Size(80, 24), Minimum = 0, Maximum = 999999, DecimalPlaces = 2 };
        var lblRub = new Label
            { Text = Strings.Get("Product_Rub"), Font = new Font("Segoe UI", 10), Location = new Point(inputX + 86, startY + 4 * rowH + 4), AutoSize = true };

        // ── Количество ────────────────────────────────────────
        _lblStock = MakeLabel(Strings.Product_LabelStock, 5);
        _txtStock = new TextBox
        {
            Font      = new Font("Segoe UI", 10),
            Location  = new Point(inputX, startY + 5 * rowH),
            Size      = new Size(60, 24),
            Text      = isEdit ? _existing!.Stock.ToString() : "0",
            ReadOnly  = isEdit,
            BackColor = isEdit ? Color.FromArgb(220, 220, 220) : Color.White
        };
        var lblPcs = new Label
            { Text = Strings.Get("Product_Pcs"), Font = new Font("Segoe UI", 10), Location = new Point(inputX + 66, startY + 5 * rowH + 4), AutoSize = true };

        // ── Срок годности ─────────────────────────────────────
        _lblExpiry = MakeLabel(Strings.Product_LabelExpiry, 6);
        _dtpExpiry = new DateTimePicker
        {
            Font     = new Font("Segoe UI", 10),
            Location = new Point(inputX, startY + 6 * rowH),
            Size     = new Size(130, 24),
            Format   = DateTimePickerFormat.Short
        };
        _chkNoExpiry = new CheckBox
        {
            Text     = Strings.Product_ChkNoExpiry,
            Font     = new Font("Segoe UI", 10),
            Location = new Point(inputX + 140, startY + 6 * rowH + 3),
            AutoSize = true
        };
        _chkNoExpiry.CheckedChanged += (s, e) => _dtpExpiry.Enabled = !_chkNoExpiry.Checked;

        var lblRequired = new Label
        {
            Text      = Strings.Product_RequiredHint,
            Font      = new Font("Segoe UI", 8),
            ForeColor = Color.Gray,
            Location  = new Point(labelX, startY + 7 * rowH + 5),
            AutoSize  = true
        };

        var btnY = startY + 7 * rowH + 28;
        _btnSave = new Button
        {
            Text      = Strings.Get("Save"),
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Location  = new Point(160, btnY),
            Size      = new Size(100, 32),
            BackColor = Color.FromArgb(28, 42, 74),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Click += BtnSave_Click;
        AcceptButton    = _btnSave;

        _btnCancel = new Button
        {
            Text      = Strings.Get("Cancel"),
            Font      = new Font("Segoe UI", 10),
            Location  = new Point(270, btnY),
            Size      = new Size(90, 32),
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand
        };
        _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        CancelButton      = _btnCancel;

        Controls.AddRange(new Control[]
        {
            _lblArticle, _txtArticle, _lblArticleError,
            _lblName, _txtName, _lblNameError,
            _lblCategory, _cmbCategory,
            _lblUnit, _cmbUnit,
            _lblPrice, _nudPrice, lblRub,
            _lblStock, _txtStock, lblPcs,
            _lblExpiry, _dtpExpiry, _chkNoExpiry,
            lblRequired, _btnSave, _btnCancel
        });
    }

    private void LoadCategories()
    {
        using var db = new AppDbContext();
        var cats = db.Categories.OrderBy(c => c.Name).ToList();
        _cmbCategory.Items.Clear();
        foreach (var c in cats) _cmbCategory.Items.Add(c.Name);
        if (_cmbCategory.Items.Count > 0) _cmbCategory.SelectedIndex = 0;
    }

    private void FillFields()
    {
        _txtArticle.Text  = _existing!.Article;
        _txtName.Text     = _existing.Name;
        _cmbCategory.Text = _existing.Category?.Name ?? string.Empty;
        _cmbUnit.Text     = _existing.Unit;
        _nudPrice.Value   = _existing.PurchasePrice;
        _txtStock.Text    = _existing.Stock.ToString();

        if (_existing.ExpiryDate.HasValue)
            _dtpExpiry.Value = _existing.ExpiryDate.Value.ToDateTime(TimeOnly.MinValue);
        else
            _chkNoExpiry.Checked = true;
    }

    private void SetFieldError(TextBox txt, bool hasError)
    {
        txt.BackColor = hasError
            ? Color.FromArgb(255, 220, 220)
            : (txt.ReadOnly ? Color.FromArgb(220, 220, 220) : Color.White);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        _lblArticleError.Visible = false;
        _lblNameError.Visible    = false;
        SetFieldError(_txtArticle, false);
        SetFieldError(_txtName, false);

        var valid = true;
        if (string.IsNullOrWhiteSpace(_txtArticle.Text))
        {
            SetFieldError(_txtArticle, true);
            _lblArticleError.Text    = Strings.RequiredField;
            _lblArticleError.Visible = true;
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            SetFieldError(_txtName, true);
            _lblNameError.Text    = Strings.RequiredField;
            _lblNameError.Visible = true;
            valid = false;
        }
        if (_cmbCategory.SelectedIndex < 0) valid = false;
        if (!valid) return;

        try
        {
            using var db = new AppDbContext();
            var category = db.Categories.FirstOrDefault(c => c.Name == _cmbCategory.SelectedItem!.ToString());
            if (category == null) return;

            var expiry = _chkNoExpiry.Checked
                ? (DateOnly?)null
                : DateOnly.FromDateTime(_dtpExpiry.Value);

            if (_existing == null)
            {
                var article = _txtArticle.Text.Trim();
                if (db.Products.Any(p => p.Article == article))
                {
                    _lblArticleError.Text    = Strings.Product_ErrArticleExists;
                    _lblArticleError.Visible = true;
                    SetFieldError(_txtArticle, true);
                    return;
                }

                db.Products.Add(new Product
                {
                    Article       = article,
                    Name          = _txtName.Text.Trim(),
                    CategoryId    = category.Id,
                    Unit          = _cmbUnit.SelectedItem!.ToString()!,
                    PurchasePrice = _nudPrice.Value,
                    Stock         = int.TryParse(_txtStock.Text, out var stock) ? stock : 0,
                    ExpiryDate    = expiry
                });
                _log.Info("Добавлен товар: {0}", article);
            }
            else
            {
                var p = db.Products.Find(_existing.Id);
                if (p == null) return;
                p.Name          = _txtName.Text.Trim();
                p.CategoryId    = category.Id;
                p.Unit          = _cmbUnit.SelectedItem!.ToString()!;
                p.PurchasePrice = _nudPrice.Value;
                p.ExpiryDate    = expiry;
                _log.Info("Обновлён товар: {0}", p.Article);
            }

            db.SaveChanges();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Ошибка сохранения товара");
            MessageBox.Show($"{Strings.Error}: {ex.Message}",
                Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
