using Bugsnag;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EarTrumpet.Diagnosis
{
    class ErrorReporter
    {
        private static ErrorReporter s_instance;
        private readonly Client _bugsnagClient;
        private readonly CircularBufferTraceListener _listener;

        public ErrorReporter()
        {
            s_instance = this;
            _listener = new CircularBufferTraceListener();
            Trace.Listeners.Clear();
            Trace.Listeners.Add(_listener);

            try
            {
                _bugsnagClient = new Client(Bugsnag.ConfigurationSection.Configuration.Settings);
                _bugsnagClient.BeforeNotify(new Middleware(OnBeforeNotify));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void DisplayDiagnosticData()
        {
            LocalDataExporter.DumpAndShowData(_listener.GetLogText());
        }

        public static void LogWarning(Exception ex) => s_instance.LogWarningInstance(ex);
        private void LogWarningInstance(Exception ex)
        {
            Trace.WriteLine($"## Warning Notify ##: {ex}");
            _bugsnagClient.Notify(ex, Severity.Warning);
        }

        private void OnBeforeNotify(Bugsnag.Payload.Report error)
        {
            try
            {
                foreach (var evt in (Bugsnag.Payload.Event[])error["events"])
                {
                    // Remove default properties that we don't need.
                    evt.Device.Clear();
                    Fill(evt.Device, SnapshotData.Device);

                    Fill(evt.App, SnapshotData.App);

                    var appSettings = new Dictionary<string, object>();
                    evt.Metadata.Add("AppSettings", appSettings);
                    Fill(appSettings, SnapshotData.AppSettings);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"## ErrorReporter OnBeforeNotify: {ex}");
            }
        }

        private void Fill(Dictionary<string, object> dest, Dictionary<string, Func<object>> source)
        {
            foreach (var key in source.Keys)
            {
                dest[key] = SnapshotData.InvokeNoThrow(source[key]);
            }
        }
    }
}
