using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using EarTrumpet.Actions.Interop.Helpers;
using Windows.Win32;

namespace EarTrumpet.Actions.DataModel;

public sealed class ProcessWatcher : IDisposable
{
    private class ProcessInfo
    {
        public List<IntPtr> Windows = [];
    }

    private class WatcherInfo
    {
        public List<Action> StartCallbacks = [];
        public List<Action> StopCallbacks = [];
        public Dictionary<int, ProcessInfo> RunningProcesses = [];
    }

    public static ProcessWatcher Current { get; } = new ProcessWatcher();

    private readonly WindowWatcher _watcher = new();
    private Dictionary<string, WatcherInfo> _info = [];

    public ProcessWatcher()
    {
        _watcher.WindowCreated += OnWindowCreated;
    }

    // Used only by the condition processor, so we use realtime data only.
    public static bool IsRunning(string procName)
    {
        try
        {
            return Process.GetProcessesByName(procName).Length != 0;
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
            var pid = 0U;
            unsafe
            {
                _ = PInvoke.GetWindowThreadProcessId(new HWND(hwnd.ToPointer()), &pid);
            }

            using var proc = Process.GetProcessById((int)pid);
            if (_info.ContainsKey(proc.ProcessName.ToLowerInvariant()))
            {
                FoundNewRelevantProcess(proc);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    private bool FoundNewRelevantProcess(Process proc)
    {
        var info = _info[proc.ProcessName.ToLowerInvariant()];

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
        text = text.ToLower(CultureInfo.CurrentCulture);
        var info = _info.TryGetValue(text, out var value) ? value : _info[text] = new WatcherInfo();
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
        text = text.ToLower(CultureInfo.CurrentCulture);
        var info = _info.TryGetValue(text, out var value) ? value : new WatcherInfo();
        info.StartCallbacks.Add(callback);

        try
        {
            var didSignal = false;
            var runningProcs = Process.GetProcessesByName(text);
            foreach (var proc in runningProcs)
            {
                didSignal = didSignal || FoundNewRelevantProcess(proc);
            }

            if (runningProcs.Length != 0 && !didSignal)
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
        _info = [];
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }
}
