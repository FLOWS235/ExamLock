using System.Collections.Generic;
using Microsoft.Win32;

namespace ExamLock
{
    public sealed class PolicyManager : System.IDisposable
    {
        private readonly Dictionary<(RegistryKey root, string subKey, string valueName), object?> _originals = new();

        public void Apply()
        {
            SetDword(@"Software\Microsoft\Windows\CurrentVersion\Policies\System",   "DisableTaskMgr",         1);
            SetDword(@"Software\Microsoft\Windows\CurrentVersion\Policies\System",   "DisableLockWorkstation", 1);
            SetDword(@"Software\Microsoft\Windows\CurrentVersion\Policies\System",   "DisableChangePassword",  1);
            SetDword(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoLogoff",               1);
        }

        public void Revert()
        {
            foreach (var kv in _originals)
            {
                var (root, subKey, name) = kv.Key;
                var prev = kv.Value;

                using var k = root.CreateSubKey(subKey, true);
                if (k == null) continue;

                if (prev is null)
                    k.DeleteValue(name, false);
                else
                    k.SetValue(name, prev, RegistryValueKind.DWord);
            }
        }

        private void SetDword(string subKey, string name, int value)
        {
            var keyTuple = (Registry.CurrentUser, subKey, name);

            using (var k = Registry.CurrentUser.CreateSubKey(subKey, true))
            {
                if (k == null) return;
                if (!_originals.ContainsKey(keyTuple))
                    _originals[keyTuple] = k.GetValue(name);

                k.SetValue(name, value, RegistryValueKind.DWord);
            }
        }

        public void Dispose() => Revert();
    }
}
