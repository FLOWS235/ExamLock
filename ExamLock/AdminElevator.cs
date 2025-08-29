using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace ExamLock
{
    public static class AdminElevator
    {
        public static bool IsRunningAsAdmin()
        {
            using var id = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool RelaunchAsAdmin()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
