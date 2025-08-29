using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ExamLock
{
    public class ExamForm : Form
    {
        private readonly DateTime _endAt;
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };
        private readonly Label _lblClock = new() { Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
        private readonly string _user;

        // Soru veri modeli
        private class Q
        {
            public string Text { get; set; } = "";
            public string[] Choices { get; set; } = Array.Empty<string>(); // 5 eleman (A..E)
        }

        private readonly Q[] _questions;
        private int _index = 0;
        private readonly RadioButton[] _opts = new RadioButton[5];
        private readonly Label _lblQuestion = new();
        private readonly FlowLayoutPanel _choicesRow = new();

        public ExamForm(string user, TimeSpan duration)
        {
            _user = user;
            _endAt = DateTime.UtcNow.Add(duration);

            // 10 matematik sorusu (örnek)
            _questions = new[]
            {
                new Q{ Text="1) 2 + 2 = ?", Choices=new[]{"1","2","3","4","5"} },
                new Q{ Text="2) 9 − 4 = ?", Choices=new[]{"3","4","5","6","7"} },
                new Q{ Text="3) 3 × 7 = ?", Choices=new[]{"18","20","21","24","27"} },
                new Q{ Text="4) 56 ÷ 7 = ?", Choices=new[]{"6","7","8","9","10"} },
                new Q{ Text="5) 15% van 200 = ?", Choices=new[]{"20","25","30","35","40"} },
                new Q{ Text="6) √144 = ?", Choices=new[]{"10","11","12","13","14"} },
                new Q{ Text="7) 5² + 3² = ?", Choices=new[]{"25","28","29","30","34"} },
                new Q{ Text="8) 0,25 × 0,4 = ?", Choices=new[]{"0,01","0,05","0,1","0,6","1"} },
                new Q{ Text="9) Mediaan van {2, 4, 4, 7, 9} = ?", Choices=new[]{"2","4","5","7","9"} },
                new Q{ Text="10) 8! / (6! × 2!) = ?", Choices=new[]{"8","14","28","56","112"} },
            };

            Text = "Examen";
            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            BackColor = Color.White;

            Controls.Add(_lblClock);

            // Dış çerçeve – merkezde düzen
            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                Padding = new Padding(40)
            };
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 80));
            Controls.Add(outer);

            // Ortadaki panel
            var center = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 1,
                Padding = new Padding(10),
                Anchor = AnchorStyles.None
            };
            outer.Controls.Add(center, 1, 1);

            // Soru – yatay tek satır
            _lblQuestion.AutoSize = true;
            _lblQuestion.Font = new Font("Segoe UI Semibold", 18);
            _lblQuestion.Margin = new Padding(0, 0, 0, 12);
            center.Controls.Add(_lblQuestion);

            // Şıklar – soldan sağa A..E
            _choicesRow.FlowDirection = FlowDirection.LeftToRight;
            _choicesRow.AutoSize = true;
            _choicesRow.WrapContents = false;
            _choicesRow.Padding = new Padding(0);
            center.Controls.Add(_choicesRow);

            for (int i = 0; i < 5; i++)
            {
                _opts[i] = new RadioButton
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 14),
                    Margin = new Padding(i == 0 ? 0 : 24, 4, 0, 4)
                };
                _choicesRow.Controls.Add(_opts[i]);
            }

            // Alt butonlar
            var btnRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0, 16, 0, 0)
            };
            var btnPrev = new Button { Text = "Vorige", Width = 120, Height = 40 };
            var btnNext = new Button { Text = "Volgende", Width = 120, Height = 40 };
            var btnFinish = new Button { Text = "Indienen", Width = 140, Height = 40 };
            btnRow.Controls.AddRange(new Control[] { btnPrev, btnNext, btnFinish });
            center.Controls.Add(btnRow);

            btnPrev.Click += (_, __) => { if (_index > 0) { _index--; ShowQuestion(); } };
            btnNext.Click += (_, __) => { if (_index < _questions.Length - 1) { _index++; ShowQuestion(); } };
            btnFinish.Click += (_, __) =>
            {
                // Basit log ve kapanış
                Logger.Log("exam_finish", _user, new { answered = SelectedCount() });
                MessageBox.Show("Examen ingediend (demo).", "Info");
                Close();
            };

            _timer.Tick += (_, __) =>
            {
                var left = _endAt - DateTime.UtcNow;
                if (left <= TimeSpan.Zero)
                {
                    _timer.Stop();
                    Logger.Log("exam_timeout", _user);
                    MessageBox.Show("Tijd is om. Het examen wordt afgesloten.", "Tijd om", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                }
                else
                {
                    _lblClock.Text = $"Resterende tijd: {left:hh\\:mm\\:ss}";
                }
            };
            _timer.Start();

            ShowQuestion();
        }

        private int SelectedCount() => _opts.Count(r => r.Checked);

        private void ShowQuestion()
        {
            var q = _questions[_index];
            _lblQuestion.Text = q.Text;

            // A..E etiketleri ve seçenek metinleri
            string[] letters = { "A) ", "B) ", "C) ", "D) ", "E) " };
            for (int i = 0; i < 5; i++)
            {
                _opts[i].Checked = false;
                _opts[i].Text = letters[i] + (i < q.Choices.Length ? q.Choices[i] : "");
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Alt | Keys.F4) || keyData == Keys.Escape)
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
