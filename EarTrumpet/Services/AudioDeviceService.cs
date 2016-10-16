using EarTrumpet.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EarTrumpet.Services
{
    public class AudioDeviceService : IAudioDeviceService
    {
        private static class Interop
        {
            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int RefreshAudioDevices();

            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int GetAudioDevices(ref IntPtr devices);

            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int GetAudioDeviceCount();

            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int SetDefaultAudioDevice([MarshalAs(UnmanagedType.LPWStr)] string deviceId);

            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int GetAudioDeviceVolume([MarshalAs(UnmanagedType.LPWStr)] string deviceId, out float volume);

            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int SetAudioDeviceVolume([MarshalAs(UnmanagedType.LPWStr)] string deviceId, float volume);

            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int MuteAudioDevice([MarshalAs(UnmanagedType.LPWStr)] string deviceId);

            [DllImport("EarTrumpet.Interop.dll")]
            public static extern int UnmuteAudioDevice([MarshalAs(UnmanagedType.LPWStr)] string deviceId);
        }

        public IEnumerable<AudioDeviceModel> GetAudioDevices()
        {
            Interop.RefreshAudioDevices();

            var deviceCount = Interop.GetAudioDeviceCount();
            var devices = new List<AudioDeviceModel>();

            var rawDevicesPtr = IntPtr.Zero;
            Interop.GetAudioDevices(ref rawDevicesPtr);

            var sizeOfAudioDeviceStruct = Marshal.SizeOf(typeof(InteropAudioDeviceModel));
            for (var i = 0; i < deviceCount; i++)
            {
                var window = new IntPtr(rawDevicesPtr.ToInt64() + (sizeOfAudioDeviceStruct * i));

                var device = (InteropAudioDeviceModel)Marshal.PtrToStructure(window, typeof(InteropAudioDeviceModel));
                devices.Add(new AudioDeviceModel()
                {
                    Id = device.Id,
                    DisplayName = device.DisplayName,
                    IsDefault = device.IsDefault,
                    IsMuted = device.IsMuted
                });
            }

            return devices;
        }

        public void SetDefaultAudioDevice(string deviceId)
        {
            Interop.SetDefaultAudioDevice(deviceId);
        }

        public float GetAudioDeviceVolume(string deviceId)
        {
            float volume;
            Interop.GetAudioDeviceVolume(deviceId, out volume);

            return volume;
        }
        public void SetAudioDeviceVolume(string deviceId, float volume)
        {
            Interop.SetAudioDeviceVolume(deviceId, volume);
        }

        public void MuteAudioDevice(string deviceId)
        {
            Interop.MuteAudioDevice(deviceId);
        }

        public void UnmuteAudioDevice(string deviceId)
        {
            Interop.UnmuteAudioDevice(deviceId);
        }
    }
}
