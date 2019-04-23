using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace EarTrumpet.Diagnosis
{
    class CircularBufferTraceListener : TraceListener
    {
        private const int MAX_LOG_LINES = 200;
        private readonly ConcurrentQueue<string> _log = new ConcurrentQueue<string>();
        private readonly DefaultTraceListener _defaultListener = new DefaultTraceListener();

        public override void Write(string message) => Debug.Assert(false);

        public override void WriteLine(string message)
        {
            message = $"{DateTime.Now.ToString("HH:mm:ss.fff")} {message}";

            _log.Enqueue(message + Environment.NewLine);

            while (_log.Count > MAX_LOG_LINES)
            {
                _log.TryDequeue(out var unused);
            }

            _defaultListener.WriteLine(message);
        }

        public string GetLogText()
        {
            var ret = new StringBuilder();
            foreach(var line in _log.ToArray())
            {
                ret.Append(line);
            }
            return ret.ToString();
        }
    }
}
