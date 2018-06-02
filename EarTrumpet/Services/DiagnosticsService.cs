using EarTrumpet.DataModel;
using EarTrumpet.DataModel.Internal;
using EarTrumpet.Extensions;
using EarTrumpet.Misc;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace EarTrumpet.Services
{
    class DiagnosticsService
    {
        private static IAudioDeviceManager _manager;

        public static void AdviseManager(IAudioDeviceManager manager)
        {
            _manager = manager;
        }

        public static void DumpAndShowData()
        {
            var allText = DumpDevices(_manager);
            allText += "\n\n";
            allText += AppTraceListener.Instance.Log.ToString();

            var fileName = $"{Path.GetTempFileName()}.txt";
            File.WriteAllText(fileName, allText);
            using (Process.Start(fileName)) { }
        }

        static string DumpSession(string indent, SafeAudioDeviceSession session)
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
            sb.AppendLine(indent + $"{session.DisplayName}");
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
                    foreach (SafeAudioDeviceSession rawSession in appSession.Children)
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
                sb.Append(device == manager.DefaultPlaybackDevice ? "[Playback Default] " : "");
                sb.Append(device == manager.DefaultCommunicationDevice ? "[Communications Default] " : "");
                sb.AppendLine(DumpDevice(device));
            }
            return sb.ToString();
        }
    }
}
