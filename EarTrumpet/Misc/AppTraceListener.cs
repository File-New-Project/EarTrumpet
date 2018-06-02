using System;
using System.Diagnostics;

namespace EarTrumpet.Misc
{
    class AppTraceListener : TraceListener
    {
        private DefaultTraceListener _defaultListener = new DefaultTraceListener();

        public override void Write(string message)
        {
            _defaultListener.Write(message);
        }

        public override void WriteLine(string message)
        {
            _defaultListener.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} {message}");
        }
    }
}
