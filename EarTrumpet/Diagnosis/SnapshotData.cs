using EarTrumpet.DataModel;
using EarTrumpet.Extensibility.Hosting;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Themes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using Windows.Win32;
using Windows.Win32.System.Threading;

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

        public static Dictionary<string, Func<object>> App => new()
        {
                    { "version", () => EarTrumpet.App.PackageVersion.ToString() },
                    { "runtimeMinutes", () => (int)EarTrumpet.App.Duration.TotalMinutes },
                    { "gdiObjects", () => PInvoke.GetGuiResources(PInvoke.GetCurrentProcess(), GET_GUI_RESOURCES_FLAGS.GR_GDIOBJECTS) },
                    { "userObjects", () => PInvoke.GetGuiResources(PInvoke.GetCurrentProcess(), GET_GUI_RESOURCES_FLAGS.GR_USEROBJECTS) },
                    { "globalGdiObjects", () => PInvoke.GetGuiResources(new HANDLE(PInvoke.GR_GLOBAL), GET_GUI_RESOURCES_FLAGS.GR_USEROBJECTS) },
                    { "globalUserObjects", () => PInvoke.GetGuiResources(new HANDLE(PInvoke.GR_GLOBAL), GET_GUI_RESOURCES_FLAGS.GR_USEROBJECTS) },
                    { "handleCount", () => GetProcessHandleCount() },
#if DEBUG
                    { "releaseStage", () => "development" },
#else
                    { "releaseStage", () => "production" },
#endif
                };

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
                    { "AnimationsEnabled", () => Manager.Current.AnimationsEnabled },
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
                    { "systemDpi", () => PInvoke.GetDpiForSystem() },
                    { "taskbarDpi", () => WindowsTaskbar.Dpi },
                    { "addons", () => AddonManager.GetDiagnosticInfo() },
                    { "region", () =>  new RegionInfo(CultureInfo.CurrentCulture.LCID).TwoLetterISORegionName }
                };
            }
        }

        private static uint GetProcessHandleCount()
        {
            var handleCount = 0U;
            unsafe
            {
                _ = PInvoke.GetProcessHandleCount(PInvoke.GetCurrentProcess(), &handleCount);
            }
            return handleCount;
        }
    }
}
