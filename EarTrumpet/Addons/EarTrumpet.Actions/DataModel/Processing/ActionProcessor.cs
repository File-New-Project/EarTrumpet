using System;
using System.Diagnostics;
using System.Linq;
using EarTrumpet.Actions.DataModel.Enum;
using EarTrumpet.Actions.DataModel.Serialization;
using EarTrumpet.DataModel.Audio;
using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.Extensions;

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
                    var app = ForegroundAppResolver.FindForegroundApp(device.Groups);
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
                    var app = ForegroundAppResolver.FindForegroundApp(device.Groups);
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
        try
        {
            switch (part.Unit)
            {
                case VolumeUnit.Percentage:
                    {
                        var vol = (float)(part.Volume / 100);
                        var prevVol = stream.GetVolumeScalar();
                        stream.SetVolumeScalar(action switch {
                            SetVolumeKind.Set => vol,
                            SetVolumeKind.Increment => (prevVol + vol).Bound(0, 1),
                            SetVolumeKind.Decrement => (prevVol - vol).Bound(0, 1),
                            _ => throw new ArgumentException("Invalid volume action.")
                        });
                    }
                    break;
                case VolumeUnit.Decibel:
                    {
                        var vol = (float)part.Volume;
                        var prevVol = stream.GetVolumeLogarithmic();
                        stream.SetVolumeLogarithmic(action switch {
                            SetVolumeKind.Set => vol,
                            SetVolumeKind.Increment => (prevVol + vol).Bound(App.Settings.LogarithmicVolumeMinDb, 0),
                            SetVolumeKind.Decrement => (prevVol - vol).Bound(App.Settings.LogarithmicVolumeMinDb, 0),
                            _ => throw new ArgumentException("Invalid volume action.")
                        });
                    }
                    break;
                default:
                    throw new ArgumentException("Invalid volume unit.");
            }
        }
        catch (Exception ex) when (ex.Is(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED))
        {
            // Expected in some cases.
        }
    }
}
