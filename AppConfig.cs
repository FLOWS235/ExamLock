using System;
using System.IO;
using System.Text.Json;

namespace ExamLock
{
    public class AppConfig
    {
        public string[] Whitelist { get; set; } = Array.Empty<string>();
        public string RemoteWhitelistUrl { get; set; } = "";
        public string ExamTitle { get; set; } = "Examen";
        public bool AllowVm { get; set; } = false; // <-- VM'e test amacıyla izin

        public static AppConfig Load(string path)
        {
            try
            {
                if (!File.Exists(path)) return new AppConfig();
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch
            {
                return new AppConfig();
            }
        }

        public static void Save(string path, AppConfig cfg)
        {
            var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
