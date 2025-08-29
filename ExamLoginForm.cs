using System.Drawing;
using System.Windows.Forms;

namespace ExamLock
{
    public class ExamLoginForm : Form
    {
        private readonly string _hwHash;

        public ExamLoginForm(string title, string hwHash)
        {
            _hwHash = hwHash;

            Text = title;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Bounds = Screen.PrimaryScreen?.Bounds ?? Screen.AllScreens[0].Bounds;
            BackColor = Color.White;

            // Dış ızgara – ortalama
            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 4,
                Padding = new Padding(40)
            };
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 80));
            Controls.Add(outer);

            var titleLbl = new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                Anchor = AnchorStyles.None
            };
            outer.Controls.Add(titleLbl, 1, 0);

            // Ortadaki giriş paneli
            var center = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(10),
                Anchor = AnchorStyles.None
            };
            center.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            center.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
            outer.Controls.Add(center, 1, 1);

            var lblUser = new Label { Text = "Gebruikersnaam", AutoSize = true, Anchor = AnchorStyles.Right, Margin = new Padding(0, 6, 8, 6) };
            var txtUser = new TextBox { Width = 320, Anchor = AnchorStyles.Left, Margin = new Padding(0, 2, 0, 2) };
            var lblPass = new Label { Text = "Wachtwoord", AutoSize = true, Anchor = AnchorStyles.Right, Margin = new Padding(0, 6, 8, 6) };
            var txtPass = new TextBox { Width = 320, UseSystemPasswordChar = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 2, 0, 2) };
            var btnEnter = new Button { Text = "Examen starten", Width = 320, Height = 42, Anchor = AnchorStyles.Left, Margin = new Padding(0, 10, 0, 0) };

            center.Controls.Add(lblUser, 0, 0);
            center.Controls.Add(txtUser, 1, 0);
            center.Controls.Add(lblPass, 0, 1);
            center.Controls.Add(txtPass, 1, 1);
            center.Controls.Add(btnEnter, 1, 2);

            // Alt sağ: HW hash
            var lblHw = new Label
            {
                Text = $"HW: {_hwHash}",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            var bottomPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            outer.Controls.Add(bottomPanel, 0, 3);
            outer.SetColumnSpan(bottomPanel, 3);
            bottomPanel.Controls.Add(lblHw, 2, 0);

            AcceptButton = btnEnter;

            btnEnter.Click += async (s, e) =>
            {
                var user = txtUser.Text.Trim();
                if (string.IsNullOrEmpty(user))
                {
                    MessageBox.Show("Gebruikersnaam is verplicht.", "Fout");
                    return;
                }
                Logger.Log("login_success", user);

                // ---- HOST (sunucu) IP: 192.168.137.1 ----
                //await HubClient.ConnectAsync("http://192.168.137.1:5000", _hwHash, user);

                using var clip = new ClipboardGuard(); // Pano engeli sınav boyunca
                using var exam = new ExamForm(user, System.TimeSpan.FromMinutes(30)); // 30 dk
                exam.ShowDialog();

                Program.SafeExit(); // sınav bitince uygulamadan çık
            };

            // Kullanıcı kapatamasın
            FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                    e.Cancel = true;
            };
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Alt | Keys.F4) || keyData == Keys.Escape)
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}

