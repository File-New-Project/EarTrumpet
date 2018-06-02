using EarTrumpet.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EarTrumpet.DataModel.Internal.Services
{
    // Monitors a list of processes by Process Id.
    // Uses a single background thread to monitor N processes for quit.
    class ProcessWatcherService
    {
        class ProcessWatcherData
        {
            public int processId;
            public Action<int> quitAction;
            public IntPtr processHandle;
        }

        private static readonly object _lock = new object();

        // Protected by _lock.
        private static readonly Dictionary<int, ProcessWatcherData> s_watchers = new Dictionary<int, ProcessWatcherData>();
        // Protected by _lock.
        private static bool _threadRunning;

        public static void WatchProcess(int processId, Action<int> processQuit)
        {
            lock (_lock)
            {
                if (s_watchers.Any(w => w.Value.processId == processId))
                {
                    return;
                }
            }

            Trace.WriteLine($"ProcessWatcherService WatchProcess {processId}");

            var data = new ProcessWatcherData
            {
                processId = processId,
                quitAction = processQuit,
                processHandle = Kernel32.OpenProcess(Kernel32.ProcessFlags.SYNCHRONIZE, false, processId)
            };

            lock (_lock)
            {
                s_watchers.Add(processId, data);
            }

            EnsureWatcherThreadRunning();
        }

        private static void EnsureWatcherThreadRunning()
        {
            bool needsNewThread;
            lock (_lock)
            {
                needsNewThread = !_threadRunning;
                if (needsNewThread)
                {
                    _threadRunning = true;
                }
            }

            if (needsNewThread)
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    bool quit = false;
                    while (_threadRunning && !quit)
                    {
                        IntPtr[] handles;
                        lock (_lock)
                        {
                            handles = s_watchers.Select(w => w.Value.processHandle).ToArray();
                        }

                        var returnValue = Kernel32.WaitForMultipleObjects(handles.Length, handles, false, (int)TimeSpan.FromSeconds(5).TotalMilliseconds);
                        switch(returnValue)
                        {
                            case Kernel32.WAIT_ABANDONED:
                            case Kernel32.WAIT_FAILED:
                                Debug.Assert(false);
                                break;
                            case Kernel32.WAIT_TIMEOUT:
                                // Go again
                                break;
                            default:
                                ProcessWatcherData data;
                                lock (_lock)
                                {
                                    var handle = handles[returnValue];
                                    data = s_watchers.First(w => w.Value.processHandle == handle).Value;

                                    s_watchers.Remove(data.processId);
                                    _threadRunning = s_watchers.Count > 0;
                                    quit = !_threadRunning;
                                }

                                Trace.WriteLine($"ProcessWatcherService Quit: {data.processId}");

                                data.quitAction(data.processId);

                                Kernel32.CloseHandle(data.processHandle);
                                break;
                        }
                    }
                }).Start();
            }
        }
    }
}
