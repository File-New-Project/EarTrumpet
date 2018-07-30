using EarTrumpet.DataModel;
using EarTrumpet.Extensibility;
using EarTrumpet.Extensibility.Shared;
using EarTrumpet_Actions.DataModel.Actions;
using System;
using System.Linq;

namespace EarTrumpet_Actions.DataModel
{
    class ActionProcessor
    {
        public static void Invoke(BaseAction a)
        {
            var playbackMgr = DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback);

            if (a is SetVariableAction)
            {
                var action = (SetVariableAction)a;
                Addon.Current.LocalVariables[action.Text] = action.Value;
            }
            else if (a is SetDefaultDeviceAction)
            {
                var action = (SetDefaultDeviceAction)a;
                var dev = playbackMgr.Devices.FirstOrDefault(d => d.Id == action.Device.Id);
                if (dev != null)
                {
                    playbackMgr.SetDefaultDevice(dev);
                }
            }
            else if (a is ChangeAppVolumeAction)
            {
                var action = (ChangeAppVolumeAction)a;

                if (action.Device?.Id == Device.AnyDevice.Id)
                {
                    foreach (var d in playbackMgr.Devices)
                    {
                        InvokeOnDevice(action, d);
                    }
                }
                else if (action.Device?.Id == null)
                {
                    if (playbackMgr.Default != null)
                    {
                        InvokeOnDevice(action, playbackMgr.Default);
                    }
                }
                else
                {
                    var device = playbackMgr.Devices.FirstOrDefault(d => d.Id == action.Device.Id);
                    if (device != null)
                    {
                        InvokeOnDevice(action, device);
                    }
                }
            }
            else if (a is ChangeDeviceVolumeAction)
            {
                var action = (ChangeDeviceVolumeAction)a;

                IAudioDevice device;
                if (action.Device?.Id == null)
                {
                    device = playbackMgr.Default;
                }
                else
                {
                    device = playbackMgr.Devices.FirstOrDefault(d => d.Id == action.Device.Id);
                }

                if (device != null)
                {
                    switch (action.Option)
                    {
                        case StreamActionKind.Mute:
                            device.IsMuted = true;
                            break;
                        case StreamActionKind.ToggleMute:
                            device.IsMuted = !device.IsMuted;
                            break;
                        case StreamActionKind.Unmute:
                            device.IsMuted = false;
                            break;
                        case StreamActionKind.SetVolume:
                            device.Volume = (float)(action.Volume / 100f);
                            break;
                        case StreamActionKind.Increment5:
                            device.Volume += 0.05f;
                            break;
                        case StreamActionKind.Decrement5:
                            device.Volume -= 0.05f;
                            break;
                    }
                }
            }
            else if (a is SetThemeAction)
            {
                var action = (SetThemeAction)a;
                var svc = (dynamic)ServiceBus.Get("EarTrumpet-Themes");
                if (svc != null)
                {
                    svc.Theme = action.Theme;
                }
            }
            else if (a is SetAddonEarTrumpetSettingsAction)
            {
                var action = (SetAddonEarTrumpetSettingsAction)a;

                var addonValues = ServiceBus.GetMany(KnownServices.ValueService);
                if (addonValues != null)
                {
                    var values = addonValues.Where(v => v is IValue<bool>).Select(v => (IValue<bool>)v).ToList();
                    var value = values.FirstOrDefault(v => v.Id == action.Option);
                    if (value != null)
                    {
                        value.Value = action.Value;
                    }
                }
            }
            else throw new NotImplementedException();
        }

        private static void InvokeOnDevice(ChangeAppVolumeAction action, IAudioDevice device)
        {
            var apps = device.Groups.Where(a => a.AppId == action.App.Id || action.App.Id == App.AnySession.Id);

            foreach (var app in apps)
            {
                switch (action.Option)
                {
                    case StreamActionKind.Mute:
                        app.IsMuted = true;
                        break;
                    case StreamActionKind.ToggleMute:
                        app.IsMuted = !device.IsMuted;
                        break;
                    case StreamActionKind.Unmute:
                        app.IsMuted = false;
                        break;
                    case StreamActionKind.SetVolume:
                        app.Volume = (float)(action.Volume / 100f);
                        break;
                    case StreamActionKind.Increment5:
                        app.Volume += 0.05f;
                        break;
                    case StreamActionKind.Decrement5:
                        app.Volume -= 0.05f;
                        break;
                }
            }
        }
    }
}
