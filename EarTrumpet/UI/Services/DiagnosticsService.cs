using EarTrumpet.DataModel;
using EarTrumpet.DataModel.Internal;
using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace EarTrumpet.UI.Services
{
    class DiagnosticsService
    {
        private static IAudioDeviceManager _deviceManager;

        public static void Advise(IAudioDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        public static void DumpAndShowData()
        {
            var allText = DumpDevices(_deviceManager);
            allText += Environment.NewLine;
            allText += $"BuildLabel: {SystemSettings.BuildLabel}" + Environment.NewLine;
            allText += $"IsLightTheme: {SystemSettings.IsLightTheme}" + Environment.NewLine;
            allText += $"RTL: {SystemSettings.IsRTL}" + Environment.NewLine;
            allText += $"IsTransparencyEnabled: {SystemSettings.IsTransparencyEnabled}" + Environment.NewLine;
            allText += $"UseAccentColor: {SystemSettings.UseAccentColor}" + Environment.NewLine;
            allText += $"AnimationsEnabled: {SystemParameters.MenuAnimation}" + Environment.NewLine;
            allText += Environment.NewLine;
            allText += AppTrace.Instance.Log.ToString();

            var fileName = $"{Path.GetTempFileName()}.txt";
            File.WriteAllText(fileName, allText);
            using (ProcessHelper.StartNoThrowAndLogWarning(fileName)) { }
        }

        static string DumpSession(string indent, IAudioDeviceSession session)
        {
            string flags= session.IsDesktopApp ? "Desktop " : "Modern ";

            if (session.IsSystemSoundsSession)
            {
                flags += "SystemSounds ";
            }

            bool isAlive = false;

            try
            {
                using (Process.GetProcessById(session.ProcessId)) { }
                    isAlive = true;
            }
            catch (Exception) { }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + $"{session.AppDisplayName}");
            sb.AppendLine(indent + $"{session.SessionDisplayName}");
            sb.AppendLine(indent + $"  [{session.State}]: {session.Volume.ToVolumeInt()}%{(session.IsMuted ? " (Muted)" : "")} {flags}pid:{session.ProcessId} {(!isAlive ? "(dead)" : "")}");
            sb.AppendLine(indent + $"  AppId: {session.AppId}  id={session.Id}");
            sb.AppendLine(indent + $"  IconPath: {session.IconPath}");
            sb.AppendLine(indent + $"  GroupingParam: {session.GroupingParam}");

            var persisted = session.PersistedDefaultEndPointId;
            if (!string.IsNullOrWhiteSpace(persisted))
            {
                sb.AppendLine(indent + $"  Persisted Playback Endpoint: {persisted}");
            }

            return sb.ToString();
        }

        static string DumpDevice(IAudioDevice device)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"{device.Volume.ToVolumeInt()}%{(device.IsMuted ? " (Muted)" : "")} {device.DisplayName} id={device.Id}");
            sb.AppendLine();

            foreach (AudioDeviceSessionGroup appGroup in device.Groups)
            {
                foreach (AudioDeviceSessionGroup appSession in appGroup.Children)
                {
                    foreach (IAudioDeviceSession rawSession in appSession.Children)
                    {
                        bool isOneSession = appSession.Children.Count == 1;
                        var indent = (isOneSession ? "  " : "|   ");
                        sb.Append(DumpSession(indent, rawSession));
                        sb.AppendLine(indent);
                    }
                }
                sb.AppendLine("------------------------------------------");
            }
            return sb.ToString();
        }

        static string DumpDevices(IAudioDeviceManager manager)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var device in manager.Devices)
            {
                sb.Append(device == manager.Default ? "[Playback Default] " : "");
                sb.AppendLine(DumpDevice(device));
            }
            return sb.ToString();
        }
    }
}
