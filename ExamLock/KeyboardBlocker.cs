using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ExamLock
{
    public sealed class KeyboardBlocker : IDisposable
    {
        private readonly Action? _onAdminHotkey;
        private readonly Action? _onPanicHotkey;
        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public KeyboardBlocker(Action? onAdminHotkey = null, Action? onPanicHotkey = null)
        {
            _onAdminHotkey = onAdminHotkey;
            _onPanicHotkey = onPanicHotkey;
            _proc = HookCallback;
            _hookId = SetHook(_proc);
        }

        public void Dispose()
        {
            if (_hookId != IntPtr.Zero) UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule!;
            return SetWindowsHookEx(13 /*WH_KEYBOARD_LL*/, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        private static bool IsBlocked(Keys vk, bool alt, bool ctrl, bool shift, bool win)
        {
            if (win) return true; // Win tuşu
            if (alt && (vk == Keys.Tab || vk == Keys.Escape || vk == Keys.F4)) return true; // Alt+Tab/Esc/F4
            if (ctrl && vk == Keys.Escape) return true; // Ctrl+Esc
            if (ctrl && (vk == Keys.C || vk == Keys.V || vk == Keys.X)) return true; // pano
            if (vk == Keys.PrintScreen) return true;
            return false;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                var key = (Keys)vkCode;

                bool alt = (GetKeyState(VK_MENU) & 0x8000) != 0;
                bool ctrl = (GetKeyState(VK_CONTROL) & 0x8000) != 0;
                bool shift = (GetKeyState(VK_SHIFT) & 0x8000) != 0;
                bool win = (GetKeyState(VK_LWIN) & 0x8000) != 0 || (GetKeyState(VK_RWIN) & 0x8000) != 0;

                // Yeni gizli admin: Ctrl + F10
                if (ctrl && !shift && key == Keys.F10)
                {
                    _onAdminHotkey?.Invoke();
                    return (IntPtr)1;
                }

                // Acil durum: Ctrl + Shift + Alt + End
                if (ctrl && shift && alt && key == Keys.End)
                {
                    _onPanicHotkey?.Invoke();
                    return (IntPtr)1;
                }

                if (IsBlocked(key, alt, ctrl, shift, win))
                    return (IntPtr)1; // olayı tüket
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        // WinAPI
        [DllImport("user32.dll")] private static extern short GetKeyState(int nVirtKey);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int VK_MENU = 0x12;     // Alt
        private const int VK_CONTROL = 0x11;  // Ctrl
        private const int VK_SHIFT = 0x10;    // Shift
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;
    }
}
