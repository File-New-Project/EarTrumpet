using EarTrumpet.DataModel.AppInformation;
using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Hosting;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

#if DEBUG
namespace EarTrumpet
{
    class DebugHelpers
    {
        class DebugContextMenuAddon : IEarTrumpetAddonNotificationAreaContextMenu
        {
            public IEnumerable<ContextMenuItem> NotificationAreaContextMenuItems
            {
                get
                {
                    return new List<ContextMenuItem>
                        {
                            new ContextMenuItem
                            {
                                DisplayName = "Developer options",
                                Children = new List<ContextMenuItem>
                                {
                                    new ContextMenuItem
                                    {
                                        DisplayName = "Remove all devices",
                                        Command = new RelayCommand(DebugRemoveAllDevices),
                                        Glyph = "\xE894",
                                        IsChecked = true,
                                    },
                                    new ContextMenuItem
                                    {
                                        DisplayName = "Add mock device",
                                        Command = new RelayCommand(DebugAddMockDevice),
                                        Glyph = "\xE948",
                                        IsChecked = true,
                                    },
                                    new ContextMenuSeparator(),
                                    new ContextMenuItem
                                    {
                                        DisplayName = "View colors",
                                        Command = new RelayCommand(() => ProcessHelper.StartNoThrow("EarTrumpet.ColorTool.exe")),
                                        Glyph = "\xE790",
                                        IsChecked = true,
                                    },
                                },
                            },
                        };
                }
            }
        }

        public static void Add()
        {
            AddonManager.Host.TrayContextMenuItems.Add(new DebugContextMenuAddon());
        }

        private static void DebugRemoveAllDevices()
        {
            var devManager = WindowsAudioFactory.Create(AudioDeviceKind.Playback);
            var devManagerNotify = (Interop.MMDeviceAPI.IMMNotificationClient)devManager;
            foreach (var dev in devManager.Devices.ToArray())
            {
                devManagerNotify.OnDeviceRemoved(dev.Id);
            }
            devManagerNotify.OnDefaultDeviceChanged(Interop.MMDeviceAPI.EDataFlow.eRender, Interop.MMDeviceAPI.ERole.eMultimedia, null);
            devManagerNotify.OnDefaultDeviceChanged(Interop.MMDeviceAPI.EDataFlow.eRender, Interop.MMDeviceAPI.ERole.eConsole, null);
        }

        private static void AddMockApp(IAudioDevice mockDevice, string displayName, string appId, string iconPath)
        {
            var mockApp = MakeMockApp(mockDevice, displayName, appId, iconPath);
            var mockApp2 = MakeMockApp(mockDevice, displayName, appId, iconPath);
            var mockApp3 = MakeMockApp(mockDevice, displayName, appId, iconPath);

            var group = new DataModel.WindowsAudio.Internal.AudioDeviceSessionGroup(mockDevice, mockApp);
            group.AddSession(mockApp2);
            group.AddSession(mockApp3);
            mockDevice.Groups.Add(group);
        }

        private static DataModel.Audio.Mocks.AudioDeviceSession MakeMockApp(IAudioDevice mockDevice, string displayName, string appId, string iconPath)
        {
            return new DataModel.Audio.Mocks.AudioDeviceSession(
                mockDevice,
                Guid.NewGuid().ToString(),
                displayName,
                appId,
                Environment.ExpandEnvironmentVariables(iconPath));
        }

        private static void DebugAddMockDevice()
        {
            var id = Guid.NewGuid().ToString();
            var devManager = WindowsAudioFactory.Create(AudioDeviceKind.Playback);
            var devManagerNotify = (Interop.MMDeviceAPI.IMMNotificationClient)devManager;

            var mockDevice = new DataModel.Audio.Mocks.AudioDevice(id, devManager);

            AddMockApp(mockDevice,
                "System Sounds",
                "*SystemSounds",
                AppInformationFactory.CreateForProcess(0).SmallLogoPath);
            AddMockApp(mockDevice,
                "Firefaux",
                "Firefaux",
                @"%ProgramFiles%\Mozilla Firefox\firefox.exe");
            AddMockApp(mockDevice,
                "Chr0me",
                "Chr0me",
                @"%ProgramFilesx86%\Google\Chrome\Application\chrome.exe");

            var addInfo = devManager.GetType().GetMethod("Add", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            addInfo.Invoke(devManager, new object[] { mockDevice });
        }
    }
}
#endif