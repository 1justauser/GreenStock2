using GreenStock.Resources;

namespace GreenStock.Forms;

/// <summary>
/// Диалог подтверждения удаления элемента.
/// Возвращает <see cref="DialogResult.Yes"/> при нажатии «Да»
/// и <see cref="DialogResult.No"/> при нажатии «Нет» или закрытии.
/// </summary>
public class DeleteConfirmForm : Form
{
    private Button _btnYes = null!;
    private Button _btnNo  = null!;

    /// <summary>
    /// Инициализирует диалог подтверждения.
    /// </summary>
    /// <param name="itemDescription">Описание удаляемого элемента (отображается пользователю).</param>
    public DeleteConfirmForm(string itemDescription)
    {
        Text            = Strings.Delete_Title;
        Size            = new Size(440, 280);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        BackColor       = Color.FromArgb(240, 240, 245);

        var icon = new PictureBox
        {
            Image    = SystemIcons.Warning.ToBitmap(),
            Location = new Point(30, 35),
            Size     = new Size(48, 48),
            SizeMode = PictureBoxSizeMode.StretchImage
        };

        var lblQuestion = new Label
        {
            Text     = Strings.Delete_Question,
            Font     = new Font("Segoe UI", 10),
            Location = new Point(90, 35),
            Size     = new Size(320, 24),
            AutoSize = false
        };

        var lblItem = new Label
        {
            Text     = itemDescription,
            Font     = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(90, 65),
            Size     = new Size(320, 24),
            AutoSize = false
        };

        var lblNote = new Label
        {
            Text      = Strings.ActionCannotBeUndone,
            Font      = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(80, 80, 80),
            Location  = new Point(90, 110),
            AutoSize  = true
        };

        var sep = new Panel
            { Location = new Point(15, 155), Size = new Size(395, 1), BackColor = Color.Silver };

        _btnYes = new Button
        {
            Text         = Strings.Get("Yes"),
            Font         = new Font("Segoe UI", 10, FontStyle.Bold),
            Location     = new Point(240, 170),
            Size         = new Size(70, 34),
            BackColor    = Color.FromArgb(28, 42, 74),
            ForeColor    = Color.White,
            FlatStyle    = FlatStyle.Flat,
            DialogResult = DialogResult.Yes,
            Cursor       = Cursors.Hand
        };
        _btnYes.FlatAppearance.BorderSize = 0;

        _btnNo = new Button
        {
            Text         = Strings.Get("No"),
            Font         = new Font("Segoe UI", 10),
            Location     = new Point(325, 170),
            Size         = new Size(70, 34),
            FlatStyle    = FlatStyle.Flat,
            DialogResult = DialogResult.No,
            Cursor       = Cursors.Hand
        };

        AcceptButton = _btnNo;
        CancelButton = _btnNo;

        Controls.AddRange(new Control[]
        {
            icon, lblQuestion, lblItem, lblNote, sep, _btnYes, _btnNo
        });
    }
}
