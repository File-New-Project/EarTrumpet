using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace EarTrumpet.Diagnosis
{
    internal sealed class CircularBufferTraceListener : TraceListener, IDisposable
    {
        private const int MAX_LOG_LINES = 800;
        private readonly ConcurrentQueue<string> _log = new();
        private readonly DefaultTraceListener _defaultListener = new();

        public override void Write(string message) => Debug.Assert(false);

        public override void WriteLine(string message)
        {
            var threadId = Environment.CurrentManagedThreadId;
            var idText = threadId == 1 ? "UI" : threadId.ToString().PadLeft(2, ' ') + "  ";
            message = $"{DateTime.Now:HH:mm:ss.fff} {idText} {message}";

            _log.Enqueue(message + Environment.NewLine);

            while (_log.Count > MAX_LOG_LINES)
            {
                _log.TryDequeue(out var _);
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

        public new void Dispose()
        {
            _defaultListener.Dispose();
            GC.SuppressFinalize(this);

            Dispose(true);
        }
    }
}
