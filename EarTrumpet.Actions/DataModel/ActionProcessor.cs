using EarTrumpet.DataModel;
using EarTrumpet_Actions.DataModel.Actions;
using System;
using System.Linq;

namespace EarTrumpet_Actions.DataModel
{
    class ActionProcessor
    {
        public static void Invoke(BaseAction a)
        {
            if (a is SetVariableAction)
            {
                var action = (SetVariableAction)a;
                // TODO
            }
            else if (a is SetDefaultDeviceAction)
            {
                var action = (SetDefaultDeviceAction)a;
                var dev = PlaybackDataModelHost.DeviceManager.Devices.FirstOrDefault(d => d.Id == action.Device.Id);
                if (dev != null)
                {
                    PlaybackDataModelHost.DeviceManager.Default = dev;
                }
            }
            else if (a is ChangeAppVolumeAction)
            {
                var action = (ChangeAppVolumeAction)a;

                if (action.Device.Id == Device.AnyDevice.Id)
                {
                    foreach (var d in PlaybackDataModelHost.DeviceManager.Devices)
                    {
                        InvokeOnDevice(action, d);
                    }
                }
                else if (action.Device.Id == null)
                {
                    if (PlaybackDataModelHost.DeviceManager.Default != null)
                    {
                        InvokeOnDevice(action, PlaybackDataModelHost.DeviceManager.Default);
                    }
                }
                else
                {
                    var device = PlaybackDataModelHost.DeviceManager.Devices.FirstOrDefault(d => d.Id == action.Device.Id);
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
                if (action.Device.Id == null)
                {
                    device = PlaybackDataModelHost.DeviceManager.Default;
                }
                else
                {
                    device = PlaybackDataModelHost.DeviceManager.Devices.FirstOrDefault(d => d.Id == action.Device.Id);
                }

                if (device != null)
                {
                    switch (action.Operation)
                    {
                        case ChangeDeviceVolumeActionKind.Mute:
                            device.IsMuted = true;
                            break;
                        case ChangeDeviceVolumeActionKind.ToggleMute:
                            device.IsMuted = !device.IsMuted;
                            break;
                        case ChangeDeviceVolumeActionKind.Unmute:
                            device.IsMuted = false;
                            break;
                        case ChangeDeviceVolumeActionKind.SetVolume:
                            device.Volume = (float)(action.Volume / 100f);
                            break;
                    }
                }
            }
            else throw new NotImplementedException();
        }

        private static void InvokeOnDevice(ChangeAppVolumeAction action, IAudioDevice device)
        {
            var apps = device.Groups.Where(a => a.AppId == action.DeviceSession.Id || action.DeviceSession.Id == App.AnySession.Id);

            foreach (var app in apps)
            {
                switch (action.Operation)
                {
                    case ChangeDeviceVolumeActionKind.Mute:
                        app.IsMuted = true;
                        break;
                    case ChangeDeviceVolumeActionKind.ToggleMute:
                        app.IsMuted = !device.IsMuted;
                        break;
                    case ChangeDeviceVolumeActionKind.Unmute:
                        app.IsMuted = false;
                        break;
                    case ChangeDeviceVolumeActionKind.SetVolume:
                        app.Volume = (float)(action.Volume / 100f);
                        break;
                }
            }
        }
    }
}
