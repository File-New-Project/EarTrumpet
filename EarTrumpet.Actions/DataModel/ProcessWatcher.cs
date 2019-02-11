using EarTrumpet.Interop;
using EarTrumpet_Actions.Interop.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace EarTrumpet_Actions.DataModel
{
    public class ProcessWatcher
    {
        public static ProcessWatcher Current { get; } = new ProcessWatcher();

        public event Action<string> ProcessStarted;
        public event Action<string> ProcessStopped;

        WindowWatcher _watcher = new WindowWatcher();
        Dictionary<int, string> _procs;
        Dictionary<IntPtr, int> _hwnds;

        public ProcessWatcher()
        {
            _procs = new Dictionary<int, string>();
            _hwnds = new Dictionary<IntPtr, int>();

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                AddRunningProcesses();
            }).Start();

            _watcher.WindowCreated += _watcher_WindowCreated;
            _watcher.WindowDestroyed += _watcher_WindowDestroyed;
        }

        public bool IsRunning(string procName)
        {
            return _procs.ContainsValue(procName);
        }

        private void _watcher_WindowDestroyed(IntPtr hwnd)
        {
            User32.GetWindowThreadProcessId(hwnd, out uint pid);

            if (pid == 0 && _hwnds.ContainsKey(hwnd))
            {
                OnStopped(_hwnds[hwnd]);
                _hwnds.Remove(hwnd);
            }
        }

        private void _watcher_WindowCreated(IntPtr hwnd)
        {
            try
            {
                User32.GetWindowThreadProcessId(hwnd, out uint pid);

                using (var proc = Process.GetProcessById((int)pid))
                {
                    MarkProcessRunning(proc.ProcessName.ToLower(), (int)pid);
                    _hwnds[hwnd] = (int)pid;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        void MarkProcessRunning(string procName, int pid, bool hideLog = false)
        {
            if (!_procs.ContainsKey(pid))
            {
                _procs[pid] = procName;
                if (!hideLog)
                {
                    Trace.WriteLine($"Process started: {procName}");
                }
                ProcessStarted?.Invoke(procName);
            }
        }

        void OnStopped(int pid)
        {
            if (!_procs.ContainsKey(pid))
            {
                var procName = _procs[pid];
                _procs.Remove(pid);
                Trace.WriteLine($"Process stopped: {procName}");

                ProcessStopped?.Invoke(procName);
            }
        }

        void AddRunningProcesses()
        {
            try
            {
                foreach (var p in Process.GetProcesses())
                {
                    try
                    {
                        MarkProcessRunning(p.ProcessName.ToLower(), p.Id, hideLog:true);
                        if (p.MainWindowHandle != IntPtr.Zero)
                        {
                            _hwnds[p.MainWindowHandle] = p.Id;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                    finally
                    {
                        p.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
