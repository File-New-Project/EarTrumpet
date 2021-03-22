using EarTrumpet.Interop;
using EarTrumpet.Actions.Interop.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EarTrumpet.Actions.DataModel
{
    public class ProcessWatcher
    {
        class ProcessInfo
        {
            public List<IntPtr> Windows = new List<IntPtr>();
        }

        class WatcherInfo
        {
            public List<Action> StartCallbacks = new List<Action>();
            public List<Action> StopCallbacks = new List<Action>();
            public Dictionary<int, ProcessInfo> RunningProcesses = new Dictionary<int, ProcessInfo>();
        }

        public static ProcessWatcher Current { get; } = new ProcessWatcher();

        WindowWatcher _watcher = new WindowWatcher();
        Dictionary<string, WatcherInfo> _info = new Dictionary<string, WatcherInfo>();

        public ProcessWatcher()
        {
            _watcher.WindowCreated += OnWindowCreated;
        }

        // Used only by the condition processor, so we use realtime data only.
        public bool IsRunning(string procName)
        {
            try
            {
                return Process.GetProcessesByName(procName).Any();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            return false;
        }

        private void OnWindowCreated(IntPtr hwnd)
        {
            try
            {
                User32.GetWindowThreadProcessId(hwnd, out uint pid);

                using (var proc = Process.GetProcessById((int)pid))
                {
                    if (_info.ContainsKey(proc.ProcessName.ToLower()))
                    {
                        FoundNewRelevantProcess(proc);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        bool FoundNewRelevantProcess(Process proc)
        {
            var info = _info[proc.ProcessName.ToLower()];

            if (!info.RunningProcesses.ContainsKey(proc.Id))
            {
                var procInfo = new ProcessInfo();
                info.RunningProcesses[proc.Id] = procInfo;

                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    var procName = proc.ProcessName;
                    proc.WaitForExit();
                    Trace.WriteLine($"ProcessWatcher STOP {procName}");
                    info.StopCallbacks.ForEach(s => s.Invoke());
                }).Start();

                Trace.WriteLine($"ProcessWatcher START {proc.ProcessName}");
                info.StartCallbacks.ForEach(s => s.Invoke());
                return true;
            }
            return false;
        }

        public void RegisterStop(string text, Action callback)
        {
            Trace.WriteLine($"ProcessWatcher RegisterStop {text}");
            text = text.ToLower();
            WatcherInfo info = _info.ContainsKey(text) ? _info[text] : _info[text] = new WatcherInfo();
            info.StopCallbacks.Add(callback);

            try
            {
                var runningProcs = Process.GetProcessesByName(text);
                foreach (var proc in runningProcs)
                {
                    FoundNewRelevantProcess(proc);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void RegisterStart(string text, Action callback)
        {
            Trace.WriteLine($"ProcessWatcher RegisterStart {text}");
            text = text.ToLower();
            WatcherInfo info = _info.ContainsKey(text) ? _info[text] : new WatcherInfo();
            info.StartCallbacks.Add(callback);

            try
            {
                bool didSignal = false;
                var runningProcs = Process.GetProcessesByName(text);
                foreach (var proc in runningProcs)
                {
                    didSignal = didSignal || FoundNewRelevantProcess(proc);
                }

                if (runningProcs.Any() && !didSignal)
                {
                    // We were already watching so we didn't signal but the process is running.
                    callback();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void Clear()
        {
            Trace.WriteLine("ProcessWatcher Clear");
            _info = new Dictionary<string, WatcherInfo>();
        }
    }
}
