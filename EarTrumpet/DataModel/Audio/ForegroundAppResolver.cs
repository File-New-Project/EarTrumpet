using EarTrumpet.DataModel.AppInformation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace EarTrumpet.DataModel.Audio
{
    public static class ForegroundAppResolver
    {
        public static IReadOnlyList<string> TryGetForegroundAppIds()
        {
            var hWnd = PInvoke.GetForegroundWindow();
            if (hWnd == (HWND)null)
            {
                Trace.WriteLine("ForegroundAppResolver: No Window (1)");
                return Array.Empty<string>();
            }

            var maxClassNameLength = (int)PInvoke.MAX_CLASS_NAME_LEN;
            Span<char> foregroundClassNameBuffer = stackalloc char[maxClassNameLength];
            foregroundClassNameBuffer.Clear();
            string foregroundClassName;
            unsafe
            {
                fixed (char* foregroundClassNamePtr = foregroundClassNameBuffer)
                {
                    PInvoke.GetClassName(hWnd, foregroundClassNamePtr, maxClassNameLength);
                    foregroundClassName = new PWSTR(foregroundClassNamePtr).ToString();
                }
            }

            if (foregroundClassName == "ApplicationFrameWindow")
            {
                hWnd = PInvoke.FindWindowEx(hWnd, (HWND)null, "Windows.UI.Core.CoreWindow", null);
            }

            if (hWnd == (HWND)null)
            {
                Trace.WriteLine("ForegroundAppResolver: No Window (2)");
                return Array.Empty<string>();
            }

            unsafe
            {
                uint processId;
                PInvoke.GetWindowThreadProcessId(hWnd, &processId);

                try
                {
                    var appInfo = AppInformationFactory.CreateForProcess(processId);
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
