using System;
using System.Linq;
using System.Management;
using System.Windows.Forms;

namespace ExamLock
{
    public static class SecurityGuards
    {
        public static bool CheckSecondMonitorAndWarn()
        {
            if (Screen.AllScreens.Length > 1)
            {
                MessageBox.Show(
                    "Er is een tweede scherm gedetecteerd. Koppel het extra scherm los en start opnieuw.",
                    "Tweede scherm gedetecteerd",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        // allowVm: true ise uyarı vermeden devam; false ise VM tespitinde engelle
        public static bool CheckVmAndWarn(bool allowVm)
        {
            if (!IsVm(out string why)) return true;

            if (allowVm)
            {
                // Geliştirme sırasında sadece bilgilendirme
                MessageBox.Show(
                    "Virtuele machine gedetecteerd (testmodus toegestaan).\n" + why,
                    "VM gedetecteerd", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }

            MessageBox.Show(
                "Een virtuele machine is gedetecteerd. Examen kan niet worden gestart op een VM.\n\n" + why,
                "VM gedetecteerd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        // Basit ve yaygın imzalar; false-pozitifi azaltmak için birden çok kaynaktan kontrol
        private static bool IsVm(out string reason)
        {
            reason = "";
            try
            {
                string manu = Wmi("Win32_ComputerSystem", "Manufacturer").ToLowerInvariant();
                string model = Wmi("Win32_ComputerSystem", "Model").ToLowerInvariant();
                string biosSer = Wmi("Win32_BIOS", "SerialNumber").ToLowerInvariant();
                string biosVer = Wmi("Win32_BIOS", "SMBIOSBIOSVersion").ToLowerInvariant();
                string bbMan = Wmi("Win32_BaseBoard", "Manufacturer").ToLowerInvariant();
                string bbProd = Wmi("Win32_BaseBoard", "Product").ToLowerInvariant();
                string cspName = Wmi("Win32_ComputerSystemProduct", "Name").ToLowerInvariant();
                string cspVendor = Wmi("Win32_ComputerSystemProduct", "Vendor").ToLowerInvariant();
                string cspVer = Wmi("Win32_ComputerSystemProduct", "Version").ToLowerInvariant();

                string combined = string.Join(" | ", new[] { manu, model, biosSer, biosVer, bbMan, bbProd, cspName, cspVendor, cspVer });

                // Yaygın VM imzaları
                string[] needles = {
                    "vmware", "virtualbox", "vbox", "kvm", "qemu", "xen", "parallels",
                    "virtual machine", "hyper-v", "microsoft corporation hyper-v",
                    "bochs"
                };

                bool nameHit = needles.Any(n => combined.Contains(n));
                bool hyperVRule =
                    (manu.Contains("microsoft") && (model.Contains("virtual") || cspName.Contains("virtual"))) ||
                    model.Contains("hyper-v") || cspVendor.Contains("microsoft corporation hyper-v");

                if (nameHit || hyperVRule)
                {
                    reason = $"(detectie: {combined})";
                    return true;
                }
            }
            catch
            {
                // WMI erişilemezse temkinli davran: VM sayma (engel çıkarma)
            }
            return false;
        }

        private static string Wmi(string cls, string prop)
        {
            try
            {
                using var s = new ManagementObjectSearcher($"SELECT {prop} FROM {cls}");
                foreach (ManagementObject o in s.Get())
                {
                    var v = o.Properties[prop]?.Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(v)) return v.Trim();
                }
            }
            catch { }
            return "";
        }
    }
}
