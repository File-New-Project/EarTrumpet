using EarTrumpet.DataModel;
using EarTrumpet.DataModel.Internal.Services;
using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using EarTrumpet.Interop;
using EarTrumpet_Actions.DataModel.Actions;
using EarTrumpet_Actions.DataModel.Enum;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace EarTrumpet_Actions.DataModel.Processing
{
    class ActionProcessor
    {
        public static void Invoke(BaseAction a)
        {
            Trace.WriteLine($"ActionProcessor Invoke: {a.Describe()}");
            if (a is SetVariableAction)
            {
                Addon.Current.LocalVariables[((SetVariableAction)a).Text] = ((SetVariableAction)a).Value;
            }
            else if (a is SetDefaultDeviceAction)
            {
                var mgr = DataModelFactory.CreateAudioDeviceManager(((SetDefaultDeviceAction)a).Device.Kind);

                var dev = mgr.Devices.FirstOrDefault(d => d.Id == ((SetDefaultDeviceAction)a).Device.Id);
                if (dev != null)
                {
                    mgr.SetDefaultDevice(dev);
                }
            }
            else if (a is SetAppVolumeAction)
            {
                var action = (SetAppVolumeAction)a;
                var mgr = DataModelFactory.CreateAudioDeviceManager(((SetAppVolumeAction)a).Device.Kind);

                var device = (action.Device?.Id == null) ?
                    mgr.Default : mgr.Devices.FirstOrDefault(d => d.Id == action.Device.Id);
                if (device != null)
                {
                    if (action.App.Id == App.ForegroundAppId)
                    {
                        var app = FindForegroundApp(device.Groups);
                        if (app != null)
                        {
                            DoAudioAction(action.Option, app, action);
                        }
                    }
                    else
                    {
                        foreach (var app in device.Groups.Where(app => action.App.Id == App.EveryAppId || app.AppId == action.App.Id))
                        {
                            DoAudioAction(action.Option, app, action);
                        }
                    }
                }
            }
            else if (a is SetAppMuteAction)
            {
                var action = (SetAppMuteAction)a;
                var mgr = DataModelFactory.CreateAudioDeviceManager(((SetAppVolumeAction)a).Device.Kind);

                var device = (action.Device?.Id == null) ?
                    mgr.Default : mgr.Devices.FirstOrDefault(d => d.Id == action.Device.Id);
                if (device != null)
                {
                    if (action.App.Id == App.ForegroundAppId)
                    {
                        var app = FindForegroundApp(device.Groups);
                        if (app != null)
                        {
                            DoAudioAction(action.Option, app);
                        }
                    }
                    else
                    {
                        foreach (var app in device.Groups.Where(app => action.App.Id == App.EveryAppId || app.AppId == action.App.Id))
                        {
                            DoAudioAction(action.Option, app);
                        }
                    }
                }
            }
            else if (a is SetDeviceVolumeAction)
            {
                var action = (SetDeviceVolumeAction)a;

                var mgr = DataModelFactory.CreateAudioDeviceManager(((SetDeviceVolumeAction)a).Device.Kind);

                var device = (action.Device?.Id == null) ?
                    mgr.Default : mgr.Devices.FirstOrDefault(d => d.Id == action.Device.Id);
                if (device != null)
                {
                    DoAudioAction(action.Option, device, action);
                }
            }
            else if (a is SetDeviceMuteAction)
            {
                var action = (SetDeviceMuteAction)a;

                var mgr = DataModelFactory.CreateAudioDeviceManager(((SetDeviceMuteAction)a).Device.Kind);

                var device = (action.Device?.Id == null) ?
                    mgr.Default : mgr.Devices.FirstOrDefault(d => d.Id == action.Device.Id);
                if (device != null)
                {
                    DoAudioAction(action.Option, device);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static IAudioDeviceSession FindForegroundApp(ObservableCollection<IAudioDeviceSession> groups)
        {
            var hWnd = User32.GetForegroundWindow();
            var foregroundClassName = new StringBuilder(User32.MAX_CLASSNAME_LENGTH);
            User32.GetClassName(hWnd, foregroundClassName, foregroundClassName.Capacity);

            if (hWnd == IntPtr.Zero)
            {
                Trace.WriteLine($"ActionProcessor FindForegroundApp: No Window (1)");
                return null;
            }

            // ApplicationFrameWindow.exe, find the real hosted process in the child CoreWindow.
            if (foregroundClassName.ToString() == "ApplicationFrameWindow")
            {
                hWnd = User32.FindWindowEx(hWnd, IntPtr.Zero, "Windows.UI.Core.CoreWindow", IntPtr.Zero);
            }

            if (hWnd == IntPtr.Zero)
            {
                Trace.WriteLine($"ActionProcessor FindForegroundApp: No Window (2)");
                return null;
            }

            User32.GetWindowThreadProcessId(hWnd, out uint processId);

            try
            {
                var appInfo = AppInformationService.GetInformationForAppByPid((int)processId);

                foreach(var group in groups)
                {
                    if (group.AppId == appInfo.PackageInstallPath)
                    {
                        Trace.WriteLine($"ActionProcessor FindForegroundApp: {group.SessionDisplayName}");
                        return group;
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex);
            }
            Trace.WriteLine("ActionProcessor FindForegroundApp Didn't locate foreground app");
            return null;
        }

        private static void DoAudioAction(MuteKind action, IStreamWithVolumeControl stream)
        {
            switch (action)
            {
                case MuteKind.Mute:
                    stream.IsMuted = true;
                    break;
                case MuteKind.ToggleMute:
                    stream.IsMuted = !stream.IsMuted;
                    break;
                case MuteKind.Unmute:
                    stream.IsMuted = false;
                    break;
            }
        }

        private static void DoAudioAction(SetVolumeKind action, IStreamWithVolumeControl stream, IPartWithVolume part)
        {
            var vol = (float)(part.Volume / 100f);
            switch (action)
            {
                case SetVolumeKind.Set:
                    stream.Volume = vol;
                    break;
                case SetVolumeKind.Increment:
                    stream.Volume += vol;
                    break;
                case SetVolumeKind.Decrement:
                    stream.Volume -= vol;
                    break;
            }
        }
    }
}
