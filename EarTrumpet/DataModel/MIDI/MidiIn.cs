using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using System.Threading.Tasks;

namespace EarTrumpet.DataModel.MIDI
{
    public static class MidiIn
    {
        private static async Task<List<MidiInDevice>> _GetAllDevices(bool returnEmptyNames=false)
        {
            var midiInputQueryString = MidiInPort.GetDeviceSelector();
            var midiInputDevices = await DeviceInformation.FindAllAsync(midiInputQueryString);

            return (from device in midiInputDevices where returnEmptyNames || device.Name.Length != 0 select new MidiInDevice(device)).ToList();
        }

        public static List<MidiInDevice> GetAllDevices(bool returnEmptyNames=false)
        {
            var t = Task.Run(async () => await _GetAllDevices(returnEmptyNames));
            return t.Result;
        }

        public static MidiInDevice GetDeviceByName(string name)
        {
            return GetAllDevices().FirstOrDefault(device => device.Name == name);
        }
        
        public static MidiInDevice GetDeviceById(string id)
        {
            return GetAllDevices().FirstOrDefault(device => device.Id == id);
        }
    }
}