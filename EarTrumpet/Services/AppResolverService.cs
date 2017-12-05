using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Services
{
    class AppResolverService
    {
        [ComImport]
        [Guid("660b90c8-73a9-4b58-8cae-355b7f55341b")]
        class AppResolver { }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("de25675a-72de-44b4-9373-05170450c140")]
        interface IApplicationResolver
        {
            void GetAppIDForShortcut();
            void GetAppIDForShortcutObject();
            void GetAppIDForWindow();
            void GetAppIDForProcess(uint processId, [MarshalAs(UnmanagedType.LPWStr)] out string appId, out bool unk1, out bool unk2, out bool unk3);
            void GetShortcutForProcess();
            void GetBestShortcutForAppID();
            void GetBestShortcutAndAppIDForAppPath();
            void CanPinApp();
            void CanPinAppShortcut();
            void GetRelaunchProperties();
            void GenerateShortcutFromWindowProperties();
            void GenerateShortcutFromItemProperties();
            void GetLauncherAppIDForItem();
            void GetShortcutForAppID();
        }

        static IApplicationResolver _appResolver = (IApplicationResolver)new AppResolver();

        public static string GetAppIdForProcess(uint processId)
        {
            if (processId == 0)
            {
                return "GeneratedId.SystemSoundsSession";
            }

            _appResolver.GetAppIDForProcess(processId, out string appid, out bool u1, out bool u2, out bool u3);
            return appid;   
        }
    }
}
