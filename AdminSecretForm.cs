using System.Drawing;
using System.Windows.Forms;

namespace ExamLock
{
    public class AdminSecretForm : Form
    {
        private TextBox _txt;

        public AdminSecretForm()
        {
            Text = "Admin toegang";
            StartPosition = FormStartPosition.CenterParent; // önemli
            TopMost = true;                                  // önemli
            ShowInTaskbar = false;

            Width = 420; Height = 180;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            KeyPreview = true;

            var lbl = new Label
            {
                Text = "Voer admin-sleutel in:",
                Dock = DockStyle.Top, Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _txt = new TextBox
            {
                Dock = DockStyle.Top,
                UseSystemPasswordChar = true,
                Margin = new Padding(10)
            };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 46,
                Padding = new Padding(10)
            };

            var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 110 };
            var btnCancel = new Button { Text = "Annuleren", DialogResult = DialogResult.Cancel, Width = 110 };

            buttons.Controls.Add(btnOk);
            buttons.Controls.Add(btnCancel);

            Controls.Add(buttons);
            Controls.Add(_txt);
            Controls.Add(lbl);

            AcceptButton = btnOk;
            CancelButton = btnCancel;

            Shown += (_, __) => _txt.Focus();

            btnOk.Click += (_, __) =>
            {
                if (_txt.Text != "admin123")
                {
                    MessageBox.Show("Ongeldige sleutel.", "Fout",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DialogResult = DialogResult.None; // pencerede kal
                }
            };
        }
    }
}
