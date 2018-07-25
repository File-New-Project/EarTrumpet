using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading;

namespace EarTrumpet_Actions.DataModel
{
    public class ProcessWatcher
    {
        public List<string> ProcessNames { get; private set; }

        public event Action<string> ProcessStarted;
        public event Action<string> ProcessStopped;

        ManagementEventWatcher _processStartWatcher = new ManagementEventWatcher();
        ManagementEventWatcher _processStopWatcher = new ManagementEventWatcher();

        public ProcessWatcher()
        {
            ProcessNames = new List<string>();
            /*
             * Requires Administrator group :/
            _processStartWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            _processStartWatcher.EventArrived += new EventArrivedEventHandler(OnProcessStarted);
            _processStopWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            _processStopWatcher.EventArrived += new EventArrivedEventHandler(OnProcessStopped);
            _processStartWatcher.Start();
            _processStopWatcher.Start();
            */

            AddRemoveProcesses();
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                while (true)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    AddRemoveProcesses();
                }
            }).Start();
        }

        void AddRemoveProcesses()
        {
            try
            {
                var newProcessNames = new List<string>();
                foreach (var p in Process.GetProcesses())
                {
                    try
                    {
                        newProcessNames.Add(p.ProcessName.ToLower() + ".exe");
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
                ProcessNames = newProcessNames;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        void OnProcessStopped(object sender, EventArrivedEventArgs e)
        {
            var processName = (string)e.NewEvent.Properties["ProcessName"].Value;
            if (ProcessNames.Contains(processName))
            {
                ProcessNames.Remove(processName);
            }
            Trace.WriteLine($"Process stopped: {processName}");

            ProcessStopped?.Invoke(processName);
        }

        void OnProcessStarted(object sender, EventArrivedEventArgs e)
        {
            var processName = (string)e.NewEvent.Properties["ProcessName"].Value;
            ProcessNames.Add(processName);
            Trace.WriteLine($"Process started: {processName}");
            ProcessStarted?.Invoke(processName);
        }
    }
}
