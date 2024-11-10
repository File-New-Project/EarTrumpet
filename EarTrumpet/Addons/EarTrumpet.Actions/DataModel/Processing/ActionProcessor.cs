using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using EarTrumpet.Actions.DataModel.Enum;
using EarTrumpet.Actions.DataModel.Serialization;
using EarTrumpet.DataModel.AppInformation;
using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using Windows.Win32;

namespace EarTrumpet.Actions.DataModel.Processing;

internal class ActionProcessor
{
    public static void Invoke(BaseAction a)
    {
        Trace.WriteLine($"ActionProcessor Invoke: {a.GetType().Name}");
        if (a is SetVariableAction setVariableAction)
        {
            EarTrumpetActionsAddon.Current.LocalVariables[setVariableAction.Text] = (setVariableAction.Value == BoolValue.True);
        }
        else if (a is SetDefaultDeviceAction setDefaultDeviceAction)
        {
            var mgr = WindowsAudioFactory.Create((AudioDeviceKind)System.Enum.Parse(typeof(AudioDeviceKind), setDefaultDeviceAction.Device.Kind));

            var dev = mgr.Devices.FirstOrDefault(d => d.Id == setDefaultDeviceAction.Device.Id);
            if (dev != null)
            {
                mgr.Default = dev;
            }
        }
        else if (a is SetAppVolumeAction setAppVolumeAction)
        {
            var mgr = WindowsAudioFactory.Create((AudioDeviceKind)System.Enum.Parse(typeof(AudioDeviceKind), ((SetAppVolumeAction)a).Device.Kind));

            var device = (setAppVolumeAction.Device?.Id == null) ?
                mgr.Default : mgr.Devices.FirstOrDefault(d => d.Id == setAppVolumeAction.Device.Id);
            if (device != null)
            {
                if (setAppVolumeAction.App.Id == AppRef.ForegroundAppId)
                {
                    var app = FindForegroundApp(device.Groups);
                    if (app != null)
                    {
                        DoAudioAction(setAppVolumeAction.Option, app, setAppVolumeAction);
                    }
                }
                else
                {
                    foreach (var app in device.Groups.Where(app => setAppVolumeAction.App.Id == AppRef.EveryAppId || app.AppId == setAppVolumeAction.App.Id))
                    {
                        DoAudioAction(setAppVolumeAction.Option, app, setAppVolumeAction);
                    }
                }
            }
        }
        else if (a is SetAppMuteAction setAppMuteAction)
        {
            var mgr = WindowsAudioFactory.Create((AudioDeviceKind)System.Enum.Parse(typeof(AudioDeviceKind), ((SetAppMuteAction)a).Device.Kind));

            var device = (setAppMuteAction.Device?.Id == null) ?
                mgr.Default : mgr.Devices.FirstOrDefault(d => d.Id == setAppMuteAction.Device.Id);
            if (device != null)
            {
                if (setAppMuteAction.App.Id == AppRef.ForegroundAppId)
                {
                    var app = FindForegroundApp(device.Groups);
                    if (app != null)
                    {
                        DoAudioAction(setAppMuteAction.Option, app);
                    }
                }
                else
                {
                    foreach (var app in device.Groups.Where(app => setAppMuteAction.App.Id == AppRef.EveryAppId || app.AppId == setAppMuteAction.App.Id))
                    {
                        DoAudioAction(setAppMuteAction.Option, app);
                    }
                }
            }
        }
        else if (a is SetDeviceVolumeAction setDeviceVolumeAction)
        {
            var mgr = WindowsAudioFactory.Create((AudioDeviceKind)System.Enum.Parse(typeof(AudioDeviceKind), ((SetDeviceVolumeAction)a).Device.Kind));

            var device = (setDeviceVolumeAction.Device?.Id == null) ?
                mgr.Default : mgr.Devices.FirstOrDefault(d => d.Id == setDeviceVolumeAction.Device.Id);
            if (device != null)
            {
                DoAudioAction(setDeviceVolumeAction.Option, device, setDeviceVolumeAction);
            }
        }
        else if (a is SetDeviceMuteAction setDeviceMuteAction)
        {
            var mgr = WindowsAudioFactory.Create((AudioDeviceKind)System.Enum.Parse(typeof(AudioDeviceKind), ((SetDeviceMuteAction)a).Device.Kind));

            var device = (setDeviceMuteAction.Device?.Id == null) ?
                mgr.Default : mgr.Devices.FirstOrDefault(d => d.Id == setDeviceMuteAction.Device.Id);
            if (device != null)
            {
                DoAudioAction(setDeviceMuteAction.Option, device);
            }
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static IAudioDeviceSession FindForegroundApp(ObservableCollection<IAudioDeviceSession> groups)
    {
        var hWnd = PInvoke.GetForegroundWindow();
        if (hWnd == (HWND)null)
        {
            Trace.WriteLine($"ActionProcessor FindForegroundApp: No Window (1)");
            return null;
        }

        var className = string.Empty;
        unsafe
        {
            Span<char> classNameBuffer = stackalloc char[(int)PInvoke.MAX_CLASS_NAME_LEN];
            fixed (char* pClassNameBuffer = classNameBuffer)
            {
                _ = PInvoke.GetClassName(hWnd, pClassNameBuffer, classNameBuffer.Length);
                className = new PWSTR(pClassNameBuffer).ToString();
            }
        }

        // ApplicationFrameWindow.exe, find the real hosted process in the child CoreWindow.
        if (className.ToString() == "ApplicationFrameWindow")
        {
            hWnd = PInvoke.FindWindowEx(hWnd, (HWND)null, "Windows.UI.Core.CoreWindow", null);
        }
        if (hWnd == (HWND)null)
        {
            Trace.WriteLine($"ActionProcessor FindForegroundApp: No Window (2)");
            return null;
        }

        var processId = 0U;
        unsafe
        {
            _ = PInvoke.GetWindowThreadProcessId(hWnd, &processId);
        }

        try
        {
            var appInfo = AppInformationFactory.CreateForProcess(processId);

            foreach(var group in groups)
            {
                if (group.AppId == appInfo.PackageInstallPath || group.AppId == appInfo.AppId)
                {
                    Trace.WriteLine($"ActionProcessor FindForegroundApp: {group.DisplayName}");
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
