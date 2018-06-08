using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace EarTrumpet
{
    class AppTrace : TraceListener
    {
        internal static AppTrace Instance;
        internal StringBuilder Log = new StringBuilder();

        private readonly DefaultTraceListener _defaultListener = new DefaultTraceListener();
        private static Action<Exception> _warningCallback;

        public static void Initialize(Action<Exception> onWarningException)
        {
            _warningCallback = onWarningException;
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new AppTrace());
        }
        
        public static void LogWarning(Exception ex)
        {
            _warningCallback.Invoke(ex);
        }

        public static void LogWarningIfFailed(int hresult)
        {
            if (hresult == 0 || hresult == 1) // S_OK, S_FALSE
            {
                return;
            }

            _warningCallback.Invoke(new COMException($"Failed: 0x{hresult.ToString("x")}", hresult));
        }

        public AppTrace()
        {
            Debug.Assert(Instance == null);

            Instance = this;
        }

        public override void Write(string message)
        {
            _defaultListener.Write(message);
        }

        public override void WriteLine(string message)
        {
            message = $"{DateTime.Now.ToString("HH:mm:ss.fff")} {message}";

            Log.AppendLine(message);
            _defaultListener.WriteLine(message);
        }
    }
}
