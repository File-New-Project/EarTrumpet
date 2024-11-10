﻿using Bugsnag;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;

namespace EarTrumpet.Diagnosis
{
    class ErrorReporter : IDisposable
    {
        private static ErrorReporter s_instance;
        private readonly Client _bugsnagClient;
        private readonly CircularBufferTraceListener _listener;
        private readonly AppSettings _settings;

        public ErrorReporter(AppSettings settings)
        {
            Debug.Assert(s_instance == null);
            s_instance = this;

            _listener = new CircularBufferTraceListener();
            _settings = settings;
            Trace.Listeners.Clear();
            Trace.Listeners.Add(_listener);

            if (Keyboard.IsKeyDown(Key.LeftAlt))
            {
                Trace.Listeners.Add(new TextWriterTraceListener(Path.Combine(Path.GetTempPath(), "EarTrumpetTrace.log")));
                Trace.AutoFlush = true;
            }

            if (_settings.IsTelemetryEnabled)
            {
                try
                {
                    var apikey = XDocument.Load("app.config")?.XPathSelectElement("//bugsnag")?.Attribute("apiKey")?.Value;
                    if (!string.IsNullOrWhiteSpace(apikey) && apikey != "{bugsnag.apikey}")
                    {
                        _bugsnagClient = new Client(apikey);
                        _bugsnagClient.BeforeNotify(new Middleware(OnBeforeNotify));
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"ErrorReporter .ctor Failed: {ex}");
                }
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
            _bugsnagClient?.Notify(ex, Severity.Warning);
        }

        private void OnBeforeNotify(Bugsnag.Payload.Report error)
        {
            try
            {
                // Remove default properties that we don't need.
                error.Event.Device.Clear();
                Fill(error.Event.Device, SnapshotData.Device);

                Fill(error.Event.App, SnapshotData.App);

                var appSettings = new Dictionary<string, object>();
                error.Event.Metadata.Add("AppSettings", appSettings);
                Fill(appSettings, SnapshotData.AppSettings);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"## ErrorReporter OnBeforeNotify: {ex}");
            }
        }

        private static void Fill(Dictionary<string, object> dest, Dictionary<string, Func<object>> source)
        {
            foreach (var key in source.Keys)
            {
                dest[key] = SnapshotData.InvokeNoThrow(source[key]);
            }
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}
