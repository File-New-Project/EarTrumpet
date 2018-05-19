using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EarTrumpet.Services
{
    public class ProcessWatcherService
    {
        class ProcessWatcherData
        {
            public int processId;
            public Action<int> quitAction;
            public IntPtr processHandle;
        }

        private static Dictionary<int, ProcessWatcherData> s_watchers = new Dictionary<int, ProcessWatcherData>();
        private static bool _threadRunning;
        private static object _lock = new object();

        public static void WatchProcess(int processId, Action<int> processQuit)
        {
            if (s_watchers.Any(w => w.Value.processId == processId)) return;

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
            if (!_threadRunning)
            {
                _threadRunning = true;

                App.Current.Exit += (_, __) => _threadRunning = false;

                new Thread(() =>
                {
                    while (_threadRunning)
                    {
                        IntPtr[] handles;
                        lock (_lock)
                        {
                            handles = s_watchers.Select(w => w.Value.processHandle).ToArray();
                        }

                        var ret = Kernel32.WaitForMultipleObjects(handles.Length, handles, false, (int)TimeSpan.FromSeconds(5).TotalMilliseconds);

                        switch(ret)
                        {
                            case Kernel32.WAIT_ABANDONED:
                                Debug.Assert(false);
                                break;
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
                                    var handle = handles[ret];
                                    data = s_watchers.First(w => w.Value.processHandle == handle).Value;
                                    s_watchers.Remove(data.processId);

                                    _threadRunning = s_watchers.Count > 0;
                                }

                                App.Current.Dispatcher.SafeInvoke(() =>
                                {
                                    Debug.WriteLine($"Process quit: {data.processId}");
                                    data.quitAction(data.processId);
                                });
                                break;
                        }
                    }
                }).Start();
            }
        }
    }
}
