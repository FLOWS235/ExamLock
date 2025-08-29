using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace ExamLock
{
    public class AdminPanelForm : Form
    {
        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

        public AdminPanelForm()
        {
            Text = "Adminpaneel";
            Width = 900; Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            var btnRefresh = new Button { Text = "Vernieuwen", Dock = DockStyle.Top, Height = 36 };
            btnRefresh.Click += (_, __) => LoadLogs();

            Controls.Add(_grid);
            Controls.Add(btnRefresh);

            LoadLogs();
        }

        private void LoadLogs()
        {
            var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
            if (!Directory.Exists(logDir)) return;

            var files = Directory.GetFiles(logDir, "audit-*.jsonl").OrderByDescending(f => f).Take(7).ToArray();
            var table = new DataTable();
            table.Columns.Add("utc_ts");
            table.Columns.Add("event");
            table.Columns.Add("user");
            table.Columns.Add("detail");

            foreach (var f in files)
            {
                foreach (var line in File.ReadAllLines(f))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    try
                    {
                        using var doc = JsonDocument.Parse(line);
                        var root = doc.RootElement;
                        table.Rows.Add(
                            root.GetProperty("utc_ts").GetString(),
                            root.GetProperty("event").GetString(),
                            root.GetProperty("user").GetString(),
                            root.TryGetProperty("detail", out var d) ? d.ToString() : ""
                        );
                    }
                    catch { }
                }
            }
            _grid.DataSource = table;
        }
    }
}
