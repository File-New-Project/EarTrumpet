using EarTrumpet.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EarTrumpet.DataModel
{
    // Monitors a list of processes by Process Id.
    // Uses a single background thread to monitor N processes for quit.
    public class ProcessWatcherService
    {
        class ProcessWatcherData
        {
            public int processId;
            public List<Action<int>> quitActions = new List<Action<int>>();
            public IntPtr processHandle;
        }

        private static readonly object _lock = new object();

        // Protected by _lock.
        private static readonly Dictionary<int, ProcessWatcherData> s_watchers = new Dictionary<int, ProcessWatcherData>();
        // Protected by _lock.
        private static bool _threadRunning;

        public static void WatchProcess(int processId, Action<int> processQuit)
        {
            var data = new ProcessWatcherData
            {
                processId = processId,
                processHandle = Kernel32.OpenProcess(Kernel32.ProcessFlags.SYNCHRONIZE, false, processId)
            };
            data.quitActions.Add(processQuit);

            if (Kernel32.WaitForSingleObject(data.processHandle, 0) != Kernel32.WAIT_TIMEOUT)
            {
                Trace.WriteLine($"ProcessWatcherService WatchProcess Error: Unwatchable handle: {processId}");
                Kernel32.CloseHandle(data.processHandle);
                return;
            }

            lock (_lock)
            {
                if (s_watchers.ContainsKey(processId))
                {
                    // We lost the race, add our callback and clean up.
                    s_watchers[processId].quitActions.Add(processQuit);
                    Kernel32.CloseHandle(data.processHandle);
                }
                else
                {
                    // Transfer ownership
                    s_watchers.Add(processId, data);
                }
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
                            // We never expect to see WAIT_ABANDONED since we are only waiting on process handles.
                            case Kernel32.WAIT_ABANDONED:
                            // We don't expect WAIT_FAILED here since we did a test WaitForSingleObject on each handle before ingestion.
                            case Kernel32.WAIT_FAILED:
                                Debug.Assert(false);
                                // Avoid creating an infintite loop if we end up with a bad handle causing WAIT_FAILED.
                                Thread.Sleep(TimeSpan.FromSeconds(5));
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

                                foreach (var act in data.quitActions)
                                {
                                    act(data.processId);
                                }

                                Kernel32.CloseHandle(data.processHandle);
                                break;
                        }
                    }
                }).Start();
            }
        }
    }
}
