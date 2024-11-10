using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EarTrumpet.Interop;
using Windows.Win32;
using Windows.Win32.System.Threading;

namespace EarTrumpet.DataModel;

// Monitors a list of processes by Process Id.
// Uses a single background thread to monitor N processes for quit.
public class ProcessWatcherService
{
    class ProcessWatcherData
    {
        public uint processId;
        public List<Action<uint>> quitActions = [];
        public HANDLE processHandle;
    }

    private static readonly object _lock = new();

    // Protected by _lock.
    private static readonly Dictionary<uint, ProcessWatcherData> s_watchers = new();
    // Protected by _lock.
    private static bool _threadRunning;

    public static void WatchProcess(uint processId, Action<uint> processQuit)
    {
        var data = new ProcessWatcherData
        {
            processId = processId,
            processHandle = PInvoke.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_SYNCHRONIZE, false, processId)
        };
        data.quitActions.Add(processQuit);

        if (PInvoke.WaitForSingleObject(data.processHandle, 0) != WAIT_EVENT.WAIT_TIMEOUT)
        {
            Trace.WriteLine($"ProcessWatcherService WatchProcess Error: Unwatchable handle: {processId}");
            PInvoke.CloseHandle(data.processHandle);
            return;
        }

        lock (_lock)
        {
            if (s_watchers.ContainsKey(processId))
            {
                // We lost the race, add our callback and clean up.
                s_watchers[processId].quitActions.Add(processQuit);
                PInvoke.CloseHandle(data.processHandle);
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

                var quit = false;
                while (_threadRunning && !quit)
                {
                    var handles = Array.Empty<HANDLE>();
                    lock (_lock)
                    {
                        handles = s_watchers.Select(w => new HANDLE(w.Value.processHandle)).ToArray();
                    }

                    var returnValue = PInvoke.WaitForMultipleObjects(handles, false, (uint)TimeSpan.FromSeconds(5).TotalMilliseconds);
                    switch(returnValue)
                    {
                        // We never expect to see WAIT_ABANDONED since we are only waiting on process handles.
                        case WAIT_EVENT.WAIT_ABANDONED:
                        // We don't expect WAIT_FAILED here since we did a test WaitForSingleObject on each handle before ingestion.
                        case WAIT_EVENT.WAIT_FAILED:
                            Debug.Assert(false);
                            // Avoid creating an infinite loop if we end up with a bad handle causing WAIT_FAILED.
                            Thread.Sleep(TimeSpan.FromSeconds(5));
                            break;
                        case WAIT_EVENT.WAIT_TIMEOUT:
                            // Go again
                            break;
                        default:
                            ProcessWatcherData data;
                            lock (_lock)
                            {
                                var handle = handles[(uint)returnValue];
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

                            PInvoke.CloseHandle(data.processHandle);
                            break;
                    }
                }
            }).Start();
        }
    }
}
