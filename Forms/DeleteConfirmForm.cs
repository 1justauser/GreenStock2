using System;
using System.Drawing;
using System.Windows.Forms;

namespace GreenStock.Forms
{
    public class DeleteConfirmForm : Form
    {
        public DeleteConfirmForm(string itemDescription, string elementType = "элемент")
        {
            this.Text            = "Подтверждение Удаления";
            this.Size            = new Size(440, 280);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.FromArgb(240, 240, 245);

            // Warning icon
            var icon = new PictureBox
            {
                Image    = SystemIcons.Warning.ToBitmap(),
                Location = new Point(30, 35),
                Size     = new Size(48, 48),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            var lblQuestion = new Label
            {
                Text     = "Вы действительно хотите удалить товар?",
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
                Text      = "Это действие нельзя отменить.",
                Font      = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location  = new Point(90, 110),
                AutoSize  = true
            };

            var sep = new Panel { Location = new Point(15, 155), Size = new Size(395, 1), BackColor = Color.Silver };

            var btnYes = new Button
            {
                Text        = "Да",
                Font        = new Font("Segoe UI", 10, FontStyle.Bold),
                Location    = new Point(240, 170),
                Size        = new Size(70, 34),
                BackColor   = Color.FromArgb(28, 42, 74),
                ForeColor   = Color.White,
                FlatStyle   = FlatStyle.Flat,
                DialogResult= DialogResult.Yes,
                Cursor      = Cursors.Hand
            };
            btnYes.FlatAppearance.BorderSize = 0;

            var btnNo = new Button
            {
                Text        = "Нет",
                Font        = new Font("Segoe UI", 10),
                Location    = new Point(325, 170),
                Size        = new Size(70, 34),
                FlatStyle   = FlatStyle.Flat,
                DialogResult= DialogResult.No,
                Cursor      = Cursors.Hand
            };

            this.AcceptButton = btnNo;
            this.CancelButton = btnNo;

            this.Controls.AddRange(new Control[]
            {
                icon, lblQuestion, lblItem, lblNote, sep, btnYes, btnNo
            });
        }
    }
}
