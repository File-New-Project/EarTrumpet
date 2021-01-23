using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

namespace EarTrumpet.DataModel.Deej
{
    public static class DeejIn
    {
        // maps from: comports -> Actions)
        private static Dictionary<string, List<Action<List<int>>>> callbacks;
        private static List<Action<string, List<int>>> generalCallbacks;
        
        private static DeviceWatcher deviceWatcher;
        private static List<string> watchedDevices;
        
        private static Dictionary<string, string> buffers;
        private static Dictionary<string, List<int>> lastValues;
        private static List<SerialPort> serialPorts;
        
        private static bool SendCallback(string port, List<int> values)
        {
            if (lastValues[port].Count != values.Count)
            {
                lastValues[port] = values;
                return true;
            }

            if (values.Where((t, i) => t != lastValues[port][i]).Any())
            {
                lastValues[port] = values;
                return true;
            }
            return false;
        }
        
        private static void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var sp = (SerialPort) sender;
            var indata = sp.ReadExisting();

            buffers[sp.PortName] += indata;
            
            while (buffers[sp.PortName].Contains("\n"))
            {
                var data = buffers[sp.PortName].Substring(0, buffers[sp.PortName].IndexOf("\n"));
                
                try
                {
                    var channels = new List<int>();
                    foreach (var c in data.Split('|'))
                    {
                        channels.Add(int.Parse(c));
                    }

                    if (SendCallback(sp.PortName, channels))
                    {
                        if (callbacks.ContainsKey(sp.PortName))
                        {
                            foreach (var callback in callbacks[sp.PortName])
                            {
                                callback(channels);
                            }
                        }

                        foreach (var callback in generalCallbacks)
                        {
                            callback(sp.PortName, channels);
                        }
                    }
                }
                catch (Exception)
                {
                }
                
                buffers[sp.PortName] = buffers[sp.PortName].Substring(buffers[sp.PortName].IndexOf("\n") + 1);
            }
        }

        internal static void _StartListening(string port)
        {
            if (watchedDevices.Contains(port) ||!GetAllDevices().Contains(port))
            {
                return;
            }

            SerialPort sp = new SerialPort(port);
            sp.BaudRate = 9600;
            sp.Parity = Parity.None;
            sp.StopBits = StopBits.One;
            sp.DataBits = 8;
            sp.Handshake = Handshake.None;
            sp.RtsEnable = false;
            
            sp.DataReceived += DataReceived;

            if (!buffers.ContainsKey(port))
            {
                buffers.Add(port, "");
            }

            if (!lastValues.ContainsKey(port))
            {
                lastValues.Add(port, new List<int>());
            }
            
            watchedDevices.Add(port);
            sp.Open();
            
            serialPorts.Add(sp);
        }

        public static void AddCallback(string port, Action<List<int>> callback)
        {
            if (!callbacks.ContainsKey(port))
            {
                callbacks.Add(port, new List<Action<List<int>>>());
                _StartListening(port);
            }

            callbacks[port].Add(callback);
        }
        
        internal static void AddGeneralCallback(Action<string, List<int>> callback)
        {
            generalCallbacks.Add(callback);
        }

        public static List<string> GetAllDevices()
        {
            return new List<string>(SerialPort.GetPortNames());
        }
        
        private static void Added(DeviceWatcher sender, DeviceInformation args)
        {
            var commands = DeejAppBinding.Current.GetCommandControlMappings();

            foreach (var device in GetAllDevices().Where(device => !watchedDevices.Contains(device)))
            {
                foreach (var command in commands)
                {
                    var config = (DeejConfiguration) command.hardwareConfiguration;
                    if (config.Port == device)
                    {
                        _StartListening(device);
                        break;
                    }
                }
            }
        }

        private static void Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            var id = args.Id;

            foreach (var serial in serialPorts)
            {
                if (!serial.IsOpen && watchedDevices.Contains(serial.PortName))
                {
                    watchedDevices.Remove(serial.PortName);
                    serialPorts.Remove(serial);
                }
            }
        }
        
        static DeejIn()
        {
            callbacks = new Dictionary<string, List<Action<List<int>>>>();
            generalCallbacks = new List<Action<string, List<int>>>();
            
            watchedDevices = new List<string>();
            
            buffers = new Dictionary<string, string>();
            lastValues = new Dictionary<string, List<int>>();
            serialPorts = new List<SerialPort>();
            
            deviceWatcher = DeviceInformation.CreateWatcher(SerialDevice.GetDeviceSelector());
            deviceWatcher.Added += Added;
            deviceWatcher.Removed += Removed;

            deviceWatcher.Start();
        }
    }
}