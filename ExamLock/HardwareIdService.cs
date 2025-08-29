using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace ExamLock
{
    public static class HardwareIdService
    {
        public static string GetHardwareHash()
        {
            var sb = new StringBuilder();
            sb.Append(Wmi("Win32_Processor", "ProcessorId"));
            sb.Append("|").Append(Wmi("Win32_BaseBoard", "SerialNumber"));
            sb.Append("|").Append(Wmi("Win32_BIOS", "SerialNumber"));
            sb.Append("|").Append(SystemDriveSerial());
            sb.Append("|").Append(Wmi("Win32_ComputerSystemProduct", "UUID")); // ek dayanak

            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString().ToUpperInvariant()));
            return BitConverter.ToString(bytes).Replace("-", "");
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
            return "NA";
        }

        private static string SystemDriveSerial()
        {
            try
            {
                var root = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                var drive = System.IO.Path.GetPathRoot(root)?.TrimEnd('\\') ?? "C:";
                using var di = new ManagementObject($"win32_logicaldisk.deviceid=\"{drive}\"");
                di.Get();
                return di["VolumeSerialNumber"]?.ToString() ?? "NA";
            }
            catch { return "NA"; }
        }
    }
}
