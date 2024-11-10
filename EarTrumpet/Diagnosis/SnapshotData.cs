using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using EarTrumpet.DataModel;
using EarTrumpet.Extensibility.Hosting;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Themes;
using Windows.Win32;
using Windows.Win32.System.Threading;

namespace EarTrumpet.Diagnosis;

internal class SnapshotData
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
                { "globalGdiObjects", () => { unsafe { return PInvoke.GetGuiResources(new HANDLE(PInvoke.GR_GLOBAL.ToPointer()), GET_GUI_RESOURCES_FLAGS.GR_USEROBJECTS); } } },
                { "globalUserObjects", () => { unsafe { return PInvoke.GetGuiResources(new HANDLE(PInvoke.GR_GLOBAL.ToPointer()), GET_GUI_RESOURCES_FLAGS.GR_USEROBJECTS); } } },
                { "handleCount", () => GetProcessHandleCount() },
#if DEBUG
                { "releaseStage", () => "development" },
#else
                { "releaseStage", () => "production" },
#endif
            };

    public static Dictionary<string, Func<object>> Device => new()
    {
        { "osVersionBuild", () => SystemSettings.BuildLabel },
        { "osArchitecture", () => Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit" },
        { "processorCount", () => Environment.ProcessorCount + " core(s)" },
    };

    public static Dictionary<string, Func<object>> AppSettings => new()
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

    public static Dictionary<string, Func<object>> LocalOnly => new()
    {
        { "systemDpi", () => PInvoke.GetDpiForSystem() },
        { "taskbarDpi", () => WindowsTaskbar.Dpi },
        { "addons", AddonManager.GetDiagnosticInfo },
        { "region", () =>  new RegionInfo(CultureInfo.CurrentCulture.LCID).TwoLetterISORegionName }
    };

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
