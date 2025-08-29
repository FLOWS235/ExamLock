using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExamLock
{
    internal static class Program
    {
        private static PolicyManager? _policy;
        private static Control? _ui;
        private static bool _adminDialogOpen = false;

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Admin (UAC)
            if (!AdminElevator.IsRunningAsAdmin())
            {
                MessageBox.Show(
                    "Deze toepassing vereist administratorrechten om het systeembeleid toe te passen.\nKlik op 'Ja' in het volgende venster.",
                    "Beheerder vereist", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (!AdminElevator.RelaunchAsAdmin())
                {
                    MessageBox.Show("Administratorrechten zijn niet verleend. De toepassing wordt afgesloten.",
                        "Beheerder vereist", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                Environment.Exit(0);
                return;
            }

            // Politikalar uygula (HKCU)
            _policy = new PolicyManager();
            _policy.Apply();
            Application.ApplicationExit += (_, __) => { try { _policy?.Revert(); } catch { } };

            // UI invoker
            _ui = new Control();
            _ui.CreateControl();

            var cfg = AppConfig.Load("appsettings.json");
            var hwHash = HardwareIdService.GetHardwareHash();

            // Whitelist
            bool allowed = WhitelistService.CheckWhitelistAsync(hwHash, cfg).GetAwaiter().GetResult();
            if (!allowed)
            {
                using var admin = new AdminAuthForm(hwHash, cfg);
                if (admin.ShowDialog() != DialogResult.OK) return;
                cfg = AppConfig.Load("appsettings.json");
            }

            // Güvenlik kontrolleri
            if (!SecurityGuards.CheckSecondMonitorAndWarn()) { Logger.Log("second_monitor_detected"); return; }
            if (!SecurityGuards.CheckVmAndWarn()) { Logger.Log("vm_detected"); return; }if (!SecurityGuards.CheckSecondMonitorAndWarn()) { Logger.Log("second_monitor_detected"); return; }
			if (!SecurityGuards.CheckVmAndWarn(cfg.AllowVm)) // <-- AllowVm
			{
				Logger.Log("vm_detected");
				return;
			}

            // Klavye kancası: gizli admin + acil çıkış
            using var hook = new KeyboardBlocker(
                onAdminHotkey: () => InvokeOnUI(() =>
                {
                    if (_adminDialogOpen) return;
                    _adminDialogOpen = true;
                    try
                    {
                        var owner = Form.ActiveForm ?? (Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null);
                        using var dialog = new AdminSecretForm { TopMost = true, ShowInTaskbar = false };
                        var result = owner is null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
                        if (result == DialogResult.OK)
                        {
                            Logger.Log("admin_open_panel");
                            using var panel = new AdminPanelForm { TopMost = true };
                            panel.ShowDialog(owner);
                        }
                        else
                        {
                            Logger.Log("admin_wrong_key");
                        }
                    }
                    finally { _adminDialogOpen = false; }
                }),
                onPanicHotkey: () => InvokeOnUI(SafeExit) // Ctrl+Shift+Alt+End
            );

            Logger.Log("app_start");

            Application.Run(new ExamLoginForm(cfg.ExamTitle, hwHash));
        }

        public static void InvokeOnUI(Action a)
        {
            if (_ui is { IsHandleCreated: true })
            {
                try { _ui.BeginInvoke(a); } catch { a(); }
            }
            else a();
        }

        public static void SafeExit()
        {
            try { _policy?.Revert(); } catch { }
            try { Logger.Log("panic_exit"); } catch { }
            Application.Exit();
            Environment.Exit(0);
        }
    }

    // AppConfig, WhitelistService, HardwareIdService -> aynen mevcut projendeki gibi kalsın
}
