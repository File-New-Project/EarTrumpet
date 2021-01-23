using System;
using System.Collections.Generic;
using System.IO.Ports;
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

            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] != lastValues[port][i])
                {
                    lastValues[port] = values;
                    return true;
                }
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
            if (watchedDevices.Contains(port))
            {
                return;
            }
            
            SerialPort sp = new SerialPort(SerialPort.GetPortNames()[0]);
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

        internal static void AddCallback(string port, Action<List<int>> callback)
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
            foreach (var device in GetAllDevices())
            {
                if (!watchedDevices.Contains(device) && callbacks.ContainsKey(device))
                {
                    _StartListening(device);
                }
            }
        }
        
        private static void Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if (watchedDevices.Contains(args.Id))
            {
                watchedDevices.Remove(args.Id);
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