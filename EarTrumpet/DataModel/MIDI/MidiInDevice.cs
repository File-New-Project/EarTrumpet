using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;

namespace EarTrumpet.DataModel.MIDI
{
    public class MidiInDevice
    {
        private readonly DeviceInformation _device;
        private MidiInPort _inPort;
        private Dictionary<Tuple<byte, byte>, List<Action<MidiControlChangeMessage>>> _ccCallbacks;

        public string Name
        {
            get => _device.Name;
        }
        
        public string Id
        {
            get => _device.Id;
        }

        public MidiInDevice(DeviceInformation device)
        {
            _device = device;
            _ccCallbacks = new Dictionary<Tuple<byte, byte>, List<Action<MidiControlChangeMessage>>>();
        }
        
        public void AddControlChangeCallback(Action<MidiControlChangeMessage> callback, byte channel=255, byte controller=255)
        {
            if (_ccCallbacks.Count == 0)
            {
                _StartListening();
            }

            var key = new Tuple<byte, byte>(channel, controller);

            if (!_ccCallbacks.ContainsKey(key))
            {
                _ccCallbacks.Add(key, new List<Action<MidiControlChangeMessage>>());
            }
            
            _ccCallbacks[key].Add(callback);
        }

        private async Task __StartListening()
        {
            _inPort = await MidiInPort.FromIdAsync(Id);

            _inPort.MessageReceived += MessageReceived;
        }

        private void _StartListening()
        {
            var t = Task.Run(async () => await __StartListening());
            t.Wait();
        }

        private void MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            IMidiMessage received = args.Message;
            
            if (received.Type == MidiMessageType.ControlChange)
            {
                MidiControlChangeMessage msg = (MidiControlChangeMessage) received;

                var key = new Tuple<byte, byte>(msg.Channel, msg.Controller);
                if (_ccCallbacks.ContainsKey(key))
                {
                    foreach (var callback in _ccCallbacks[key])
                    {
                        callback(msg);
                    }
                }

                var universalKey = new Tuple<byte, byte>(255, 255);
                if (_ccCallbacks.ContainsKey(universalKey))
                {
                    foreach (var callback in _ccCallbacks[universalKey])
                    {
                        callback(msg);
                    }
                }
            }
            
        }
    }
}