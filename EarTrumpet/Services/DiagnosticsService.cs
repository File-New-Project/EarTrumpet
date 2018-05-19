using EarTrumpet.DataModel;
using EarTrumpet.DataModel.Internal;
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
            var fileName = $"{Path.GetTempFileName()}.txt";
            File.WriteAllText(fileName, DumpDevices(manager));
            Process.Start(fileName);
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

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"  Display Name: {session.DisplayName}");
                sb.AppendLine($"  Raw Display Name: {((SafeAudioDeviceSession)session).RawDisplayName}");
                sb.AppendLine($"  Id: {session.Id}");
                sb.AppendLine($"  AppId: {session.AppId}");
                sb.AppendLine($"  GroupingParam: {session.GroupingParam}");
                sb.AppendLine($"  Flags: {flags}");
                sb.AppendLine($"  ProcessId: {session.ProcessId}");
                sb.AppendLine($"  PersistedEndpointDeviceId: {AudioPolicyConfigService.GetDefaultEndPoint(session.ProcessId)}");
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

            foreach (var session in device.Groups)
            {
                var container = session as AudioDeviceSessionGroup;
                foreach(var sessionIncontainer in container.Children)
                {
                    sb.AppendLine(DumpSession(sessionIncontainer));
                }
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
