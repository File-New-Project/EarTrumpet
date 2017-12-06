using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace EarTrumpet.Services
{
    public class DiagnosticsService
    {
        public static void DumpAndShowData(IAudioDeviceManager manager)
        {
            var fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, DumpDevices(manager));
            Process.Start("notepad", fileName);
        }

        static string DumpSession(IAudioDeviceSession session)
        {
            string flags = "";
            if (session.IsDesktopApp)
            {
                flags += "IsDesktop ";
            }

            if (session.IsSystemSoundsSession)
            {
                flags += "IsSystemSoundsSession ";
            }

            if (session.IsHidden)
            {
                flags += "IsHidden ";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"  Display Name: {session.DisplayName}");
            sb.AppendLine($"  Id: {session.Id}");
            sb.AppendLine($"  AppID: {AppResolverService.GetAppIdForProcess((uint)session.ProcessId) }");
            sb.AppendLine($"  GroupingParam: {session.GroupingParam}");
            sb.AppendLine($"  Flags: {flags}");
            sb.AppendLine($"  ProcessId: {session.ProcessId}");
            sb.AppendLine($"  State: {session.State}");
            sb.AppendLine($"  IconPath: {session.IconPath}");
            sb.AppendLine($"  Volume: {session.Volume.ToVolumeInt()} {(session.IsMuted ? "Muted" : "")}");
            return sb.ToString();
        }

        static string DumpDevice(IAudioDevice device)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"Device: {device.DisplayName} {device.Id}");
            sb.AppendLine($"Volume: {device.Volume.ToVolumeInt()} {(device.IsMuted ? "Muted" : "")}");

            foreach (var session in device.Sessions)
            {
                sb.AppendLine(DumpSession(session));
            }
            return sb.ToString();
        }

        static string DumpDevices(IAudioDeviceManager manager)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Default Playback Device: {manager.DefaultPlaybackDevice.DisplayName} {manager.DefaultPlaybackDevice.Id}");
            foreach (var device in manager.Devices)
            {
                sb.AppendLine(DumpDevice(device));
            }
            return sb.ToString();
        }
    }
}
