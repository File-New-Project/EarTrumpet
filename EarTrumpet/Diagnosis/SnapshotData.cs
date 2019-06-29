using EarTrumpet.DataModel;
using EarTrumpet.Extensibility.Hosting;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace EarTrumpet.Diagnosis
{
    class SnapshotData
    {
        public static string InvokeNoThrow(Func<object> func)
        {
            try
            {
                return $"{func()}";
            }
            catch (Exception ex)
            {
                return $"{ex}";
            }
        }

        public static Dictionary<string, Func<object>> App
        {
            get
            {
                return new Dictionary<string, Func<object>>
                {
                    { "version", () => EarTrumpet.App.PackageVersion.ToString() },
                    { "runtimeMinutes", () => (int)EarTrumpet.App.Duration.TotalMinutes },
                    { "gdiObjects", () => User32.GetGuiResources(Kernel32.GetCurrentProcess(), User32.GR_FLAGS.GR_GDIOBJECTS) },
                    { "userObjects", () => User32.GetGuiResources(Kernel32.GetCurrentProcess(), User32.GR_FLAGS.GR_USEROBJECTS) },
                    { "globalGdiObjects", () => User32.GetGuiResources(User32.GR_GLOBAL, User32.GR_FLAGS.GR_USEROBJECTS) },
                    { "globalUserObjects", () => User32.GetGuiResources(User32.GR_GLOBAL, User32.GR_FLAGS.GR_USEROBJECTS) },
                    { "handleCount", () => GetProcessHandleCount() },
#if DEBUG
                    { "releaseStage", () => "development" },
#else
                    { "releaseStage", () => "production" },
#endif
                };
            }
        }

        public static Dictionary<string, Func<object>> Device
        {
            get
            {
                return new Dictionary<string, Func<object>>
                {
                    { "osVersionBuild", () => SystemSettings.BuildLabel },
                    { "osArchitecture", () => Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit" },
                    { "processorCount", () => Environment.ProcessorCount + " core(s)" },
                };
            }
        }

        public static Dictionary<string, Func<object>> AppSettings
        {
            get
            {
                return new Dictionary<string, Func<object>>
                {
                    { "IsLightTheme", () => SystemSettings.IsLightTheme },
                    { "IsSystemLightTheme", () => SystemSettings.IsSystemLightTheme },
                    { "IsRTL", () => SystemSettings.IsRTL },
                    { "IsTransparencyEnabled", () => SystemSettings.IsTransparencyEnabled },
                    { "UseAccentColor", () => SystemSettings.UseAccentColor },
                    { "AnimationsEnabled", () => SystemParameters.MenuAnimation },
                    { "IsHighContrast", () => SystemParameters.HighContrast },
                    { "HasIdentity", () => EarTrumpet.App.HasIdentity },
                    { "IsShuttingDown", () => EarTrumpet.App.IsShuttingDown },
                    { "Culture", () =>  CultureInfo.CurrentCulture.Name },
                    { "CurrentUICulture", () => CultureInfo.CurrentUICulture.Name },
                };
            }
        }

        public static Dictionary<string, Func<object>> LocalOnly
        {
            get
            {
                return new Dictionary<string, Func<object>>
                {
                    { "systemDpi", () => User32.GetDpiForSystem() },
                    { "taskbarDpi", () => WindowsTaskbar.Dpi },
                    { "addons", () => AddonManager.GetDiagnosticInfo() },
                    { "region", () =>  new RegionInfo(CultureInfo.CurrentCulture.LCID).TwoLetterISORegionName }
                };
            }
        }

        private static uint GetProcessHandleCount()
        {
            Kernel32.GetProcessHandleCount(Kernel32.GetCurrentProcess(), out uint handleCount);
            return handleCount;
        }
    }
}
