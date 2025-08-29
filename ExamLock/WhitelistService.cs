using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExamLock
{
    public static class WhitelistService
    {
        /// <summary>
        /// Yerel appsettings + (varsa) uzak listeden birleşik whitelist oluşturur ve karşılaştırır.
        /// </summary>
        public static async Task<bool> CheckWhitelistAsync(string hwHash, AppConfig cfg)
        {
            var list = cfg.Whitelist?.ToList() ?? [];

            if (!string.IsNullOrWhiteSpace(cfg.RemoteWhitelistUrl))
            {
                try
                {
                    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                    var txt = await http.GetStringAsync(cfg.RemoteWhitelistUrl);

                    if (txt.TrimStart().StartsWith("["))
                        list.AddRange(JsonSerializer.Deserialize<string[]>(txt) ?? []);
                    else
                        list.AddRange(txt.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                }
                catch
                {
                    // Offline veya ulaşılamıyorsa: sadece yerel listeyle devam
                }
            }

            return list.Any(x => string.Equals(x.Trim(), hwHash, StringComparison.OrdinalIgnoreCase));
        }
    }
}
