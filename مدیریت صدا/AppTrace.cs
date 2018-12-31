using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace EarTrumpet
{
    class AppTrace : TraceListener
    {
        private const int MAX_LOG_LINES = 200;
        private static readonly ConcurrentQueue<string> s_log = new ConcurrentQueue<string>();
        private static readonly DefaultTraceListener s_defaultListener = new DefaultTraceListener();

        private static AppTrace s_instance;
        private static Action<Exception> s_warningCallback;

        public static void Initialize(Action<Exception> onWarningException)
        {
            s_warningCallback = onWarningException;
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new AppTrace());
        }
        
        public static void LogWarning(Exception ex)
        {
            s_warningCallback.Invoke(ex);
        }

        public AppTrace()
        {
            Debug.Assert(s_instance == null);
            s_instance = this;
        }

        public override void Write(string message)
        {
            // We don't use Write and thus don't support the additional complexity.
            Debug.Assert(false);
        }

        public override void WriteLine(string message)
        {
            message = $"{DateTime.Now.ToString("HH:mm:ss.fff")} {message}";

            s_log.Enqueue(message + Environment.NewLine);

            while (s_log.Count > MAX_LOG_LINES)
            {
                s_log.TryDequeue(out var unused);
            }

            s_defaultListener.WriteLine(message);
        }

        public static string GetLogText()
        {
            var ret = new StringBuilder();
            foreach(var line in s_log.ToArray())
            {
                ret.Append(line);
            }
            return ret.ToString();
        }
    }
}
