using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.DataModel.WindowsAudio.Internal;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace EarTrumpet.Diagnosis
{
    public class LocalDataExporter
    {
        public static void DumpAndShowData(string logText, string addons)
        {
            var ret = new StringBuilder();
            ret.AppendLine(DumpDevices(WindowsAudioFactory.Create(AudioDeviceKind.Playback)));
            ret.AppendLine(DumpDevices(WindowsAudioFactory.Create(AudioDeviceKind.Recording)));
            Populate(ret, SnapshotData.App);
            Populate(ret, SnapshotData.Device);
            Populate(ret, SnapshotData.AppSettings);
            ret.AppendLine($"Addons: {addons}");
            ret.AppendLine();
            ret.AppendLine(logText);

            var fileName = $"{Path.GetTempFileName()}.txt";
            File.WriteAllText(fileName, ret.ToString());
            ProcessHelper.StartNoThrow(fileName);
        }

        private static void Populate(StringBuilder builder, Dictionary<string, Func<object>> source)
        {
            foreach (var key in source.Keys)
            {
                builder.AppendLine($"{key}: {SnapshotData.InvokeNoThrow(source[key])}");
            }
        }

        static string DumpSession(string indent, IAudioDeviceSessionInternal session)
        {
            string flags = session.IsDesktopApp ? "Desktop " : "Modern ";

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

            var persisted = ((IAudioDeviceManagerWindowsAudio)session.Parent.Parent).GetDefaultEndPoint(session.ProcessId);
            if (!string.IsNullOrWhiteSpace(persisted))
            {
                sb.AppendLine(indent + $"  Persisted Endpoint: {persisted}");
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
                        sb.Append(DumpSession(indent, (IAudioDeviceSessionInternal)rawSession));
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
                sb.Append(device == manager.Default ? $"[Default {manager.Kind}] " : "");
                sb.AppendLine(DumpDevice(device));
            }
            return sb.ToString();
        }
    }
}
