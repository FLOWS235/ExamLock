using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace ExamLock
{
    public class AdminAuthForm : Form
    {
        private readonly string _hash;
        private TextBox _txtKey;
        private Button _btnAuthorize;
        private TextBox _txtHash;

        public AdminAuthForm(string hwHash, AppConfig cfg)
        {
            _hash = hwHash ?? string.Empty;

            Text = "Onbevoegd apparaat";
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Font;
            Font = new Font("Segoe UI", 10);
            MinimumSize = new Size(720, 340);
            Size = new Size(740, 360);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(12),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(grid);

            var lblInfo = new Label
            {
                AutoSize = true,
                Text = "Dit apparaat is niet gemachtigd voor het examen.\n\nVoer de admin-sleutel in:",
                Margin = new Padding(0, 0, 0, 8)
            };
            grid.Controls.Add(lblInfo, 0, 0);

            _txtKey = new TextBox
            {
                UseSystemPasswordChar = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 8)
            };
            grid.Controls.Add(_txtKey, 0, 1);

            var buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 12)
            };

            var btnCheck = new Button { Text = "Controleren", AutoSize = true, Margin = new Padding(6, 0, 0, 0) };
            var btnCancel = new Button { Text = "Annuleren", AutoSize = true, Margin = new Padding(6, 0, 0, 0) };
            _btnAuthorize = new Button { Text = "Dit apparaat machtigen", AutoSize = true, Enabled = false };

            buttons.Controls.AddRange(new Control[] { btnCheck, btnCancel, _btnAuthorize });
            grid.Controls.Add(buttons, 0, 2);

            // HW-hash satırı
            var hashRow = new TableLayoutPanel { ColumnCount = 3, Dock = DockStyle.Top, AutoSize = true };
            hashRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            hashRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            hashRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var lblHashTitle = new Label { Text = "HW-hash:", AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(0, 6, 8, 0) };
            _txtHash = new TextBox { ReadOnly = true, BorderStyle = BorderStyle.FixedSingle, Font = new Font(FontFamily.GenericMonospace, 9.5f), Dock = DockStyle.Top, Text = string.IsNullOrWhiteSpace(_hash) ? "(leeg)" : _hash };
            var btnCopy = new Button { Text = "Kopiëren", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };

            hashRow.Controls.Add(lblHashTitle, 0, 0);
            hashRow.Controls.Add(_txtHash, 1, 0);
            hashRow.Controls.Add(btnCopy, 2, 0);
            grid.Controls.Add(hashRow, 0, 3);

            AcceptButton = btnCheck;
            CancelButton = btnCancel;

            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            btnCheck.Click += (s, e) =>
            {
                if (_txtKey.Text == "admin123")
                {
                    _btnAuthorize.Enabled = true;
                    MessageBox.Show("Admin-sleutel correct. U kunt dit apparaat machtigen of doorgaan.", "Admin");
                }
                else
                {
                    MessageBox.Show("Ongeldige sleutel.", "Fout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            _btnAuthorize.Click += (s, e) =>
            {
                var c = AppConfig.Load("appsettings.json");
                var list = c.Whitelist?.ToList() ?? [];
                if (!list.Contains(_hash, StringComparer.OrdinalIgnoreCase))
                {
                    list.Add(_hash);
                    c.Whitelist = list.ToArray();
                    AppConfig.Save("appsettings.json", c);
                    MessageBox.Show("Apparaat toegevoegd aan whitelist.", "Succes");
                }
                DialogResult = DialogResult.OK;
                Close();
            };

            btnCopy.Click += (s, e) =>
            {
                try { Clipboard.SetText(_hash ?? ""); MessageBox.Show("HW-hash gekopieerd naar klembord.", "Gekopieerd"); }
                catch { }
            };
        }
    }
}
