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
        private static readonly string LineText = "--------------------------------------------------------------------";

        public static void DumpAndShowData(string logText)
        {
            var ret = new StringBuilder();
            Populate(ret, SnapshotData.App);
            Populate(ret, SnapshotData.Device);
            Populate(ret, SnapshotData.AppSettings);
            Populate(ret, SnapshotData.LocalOnly);
            ret.AppendLine(LineText);
            DumpDeviceManager(ret, WindowsAudioFactory.Create(AudioDeviceKind.Playback));
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

        private static void DumpDeviceManager(StringBuilder builder, IAudioDeviceManager manager)
        {
            foreach (var device in manager.Devices)
            {
                DumpDevice(builder, device);
            }
        }

        private static void DumpDevice(StringBuilder builder, IAudioDevice device)
        {
            builder.Append(device == device.Parent.Default ? $"(Default Device) " : "");
            builder.AppendLine($"{device.DisplayName} {device.Volume.ToVolumeInt()}%{(device.IsMuted ? " (Muted)" : "")} Id: {device.Id}");

            foreach (AudioDeviceSessionGroup appGroup in device.Groups)
            {
                builder.AppendLine(LineText);
                foreach (AudioDeviceSessionGroup appSession in appGroup.Children)
                {
                    foreach (IAudioDeviceSession rawSession in appSession.Children)
                    {
                        DumpSession(builder,
                            appSession.Children.Count == 1 ? "  " : "| ", 
                            (IAudioDeviceSessionInternal)rawSession);
                    }
                }
            }
            builder.AppendLine(LineText);
        }

        private static void DumpSession(StringBuilder builder, string indent, IAudioDeviceSessionInternal session)
        {
            var typeText = session.IsSystemSoundsSession ? "SystemSounds" : (session.IsDesktopApp ? "Desktop" : "Modern");

            builder.AppendLine(indent + $"{session.DisplayName}");
            builder.AppendLine(indent + $"  ({typeText}) ({session.State}) {session.Volume.ToVolumeInt()}%{(session.IsMuted ? " (Muted)" : "")} Id: {session.Id}");
            builder.AppendLine(indent + $"  AppId: {session.AppId} ProcessId: {session.ProcessId} Alive: {IsProcessAlive(session.ProcessId)}");
            builder.AppendLine(indent + $"  IconPath: {session.IconPath}");
            builder.AppendLine(indent + $"  GroupingParam: {session.GroupingParam}");

            var persisted = ((IAudioDeviceManagerWindowsAudio)session.Parent.Parent).GetDefaultEndPoint(session.ProcessId);
            if (!string.IsNullOrWhiteSpace(persisted))
            {
                builder.AppendLine(indent + $"  Persisted Endpoint Id: {persisted}");
            }
        }

        private static bool IsProcessAlive(int processId)
        {
            bool isAlive = false;
            try
            {
                using (Process.GetProcessById(processId))
                {
                }
                isAlive = true;
            }
            catch (Exception)
            {
            }
            return isAlive;
        }
    }
}
