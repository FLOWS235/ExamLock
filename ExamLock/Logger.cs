using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ExamLock
{
    public static class Logger
    {
        private static readonly object _lock = new();
        private static string LogDir => Path.Combine(AppContext.BaseDirectory, "logs");
        private static string JsonlPath => Path.Combine(LogDir, $"audit-{DateTime.UtcNow:yyyyMMdd}.jsonl");
        private static string CsvPath => Path.Combine(LogDir, $"audit-{DateTime.UtcNow:yyyyMMdd}.csv");

        static Logger()
        {
            Directory.CreateDirectory(LogDir);
            if (!File.Exists(CsvPath))
            {
                File.AppendAllText(CsvPath, "utc_ts,event,user,detail\n", Encoding.UTF8);
            }
        }

        public static void Log(string evt, string user = "", object? detail = null)
        {
            var rec = new
            {
                utc_ts = DateTime.UtcNow.ToString("o"),
                @event = evt,
                user,
                detail
            };

            var json = JsonSerializer.Serialize(rec);

            lock (_lock)
            {
                File.AppendAllText(JsonlPath, json + Environment.NewLine, Encoding.UTF8);
                var csv = $"{DateTime.UtcNow:o},{Escape(evt)},{Escape(user)},{Escape(detail?.ToString() ?? "")}\n";
                File.AppendAllText(CsvPath, csv, Encoding.UTF8);
            }
        }

        private static string Escape(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";
    }
}
