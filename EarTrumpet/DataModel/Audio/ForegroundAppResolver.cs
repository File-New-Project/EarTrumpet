using EarTrumpet.DataModel.AppInformation;
using EarTrumpet.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace EarTrumpet.DataModel.Audio
{
    public static class ForegroundAppResolver
    {
        public static IReadOnlyList<string> TryGetForegroundAppIds()
        {
            var hWnd = User32.GetForegroundWindow();
            var foregroundClassName = new StringBuilder(User32.MAX_CLASSNAME_LENGTH);
            User32.GetClassName(hWnd, foregroundClassName, foregroundClassName.Capacity);

            if (hWnd == IntPtr.Zero)
            {
                Trace.WriteLine("ForegroundAppResolver: No Window (1)");
                return Array.Empty<string>();
            }

            if (foregroundClassName.ToString() == "ApplicationFrameWindow")
            {
                hWnd = User32.FindWindowEx(hWnd, IntPtr.Zero, "Windows.UI.Core.CoreWindow", IntPtr.Zero);
            }

            if (hWnd == IntPtr.Zero)
            {
                Trace.WriteLine("ForegroundAppResolver: No Window (2)");
                return Array.Empty<string>();
            }

            User32.GetWindowThreadProcessId(hWnd, out uint processId);

            try
            {
                var appInfo = AppInformationFactory.CreateForProcess((int)processId);
                return new[] { appInfo.PackageInstallPath, appInfo.AppId }
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct()
                    .ToArray();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return Array.Empty<string>();
            }
        }

        public static IAudioDeviceSession FindForegroundApp(ObservableCollection<IAudioDeviceSession> groups)
        {
            var foregroundAppIds = TryGetForegroundAppIds();
            if (foregroundAppIds.Count == 0)
            {
                return null;
            }

            var group = groups.FirstOrDefault(candidate => foregroundAppIds.Contains(candidate.AppId));
            if (group != null)
            {
                Trace.WriteLine($"ForegroundAppResolver: {group.DisplayName}");
            }
            else
            {
                Trace.WriteLine("ForegroundAppResolver: Didn't locate foreground app");
            }

            return group;
        }

        public static IAudioDeviceSession FindForegroundApp(IEnumerable<IAudioDevice> devices)
        {
            var foregroundAppIds = TryGetForegroundAppIds();
            if (foregroundAppIds.Count == 0)
            {
                return null;
            }

            foreach (var device in devices)
            {
                var group = device.Groups.FirstOrDefault(candidate => foregroundAppIds.Contains(candidate.AppId));
                if (group != null)
                {
                    Trace.WriteLine($"ForegroundAppResolver: {group.DisplayName}");
                    return group;
                }
            }

            Trace.WriteLine("ForegroundAppResolver: Didn't locate foreground app");
            return null;
        }
    }
}