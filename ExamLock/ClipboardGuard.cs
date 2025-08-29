using System;
using System.Windows.Forms;

namespace ExamLock
{
    public sealed class ClipboardGuard : IDisposable
    {
        private readonly System.Windows.Forms.Timer _t;

        public ClipboardGuard(int intervalMs = 400)
        {
            _t = new System.Windows.Forms.Timer { Interval = intervalMs };
            _t.Tick += (_, __) =>
            {
                try { Clipboard.Clear(); } catch { }
            };
            _t.Start();
        }

        public void Dispose() => _t.Dispose();
    }
}
