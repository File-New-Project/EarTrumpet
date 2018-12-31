using System;
using System.Windows.Forms;

namespace EarTrumpet.Interop.Helpers
{
    public class Win32Window : NativeWindow, IDisposable
    {
        Action<Message> _wndProc;

        public void Initialize(Action<Message> wndProc)
        {
            _wndProc = wndProc;
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            _wndProc(m);
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }
}
