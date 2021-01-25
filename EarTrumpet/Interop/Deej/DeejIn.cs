using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

namespace EarTrumpet.DataModel.Deej
{
    public static class DeejIn
    {
        // maps from: comports -> Actions)
        private static ConcurrentDictionary<string, List<Action<List<int>>>> callbacks;
        private static List<Action<string, List<int>>> generalCallbacks;
        
        private static DeviceWatcher deviceWatcher;
        private static List<string> watchedDevices;
        
        // port -> buffer
        private static ConcurrentDictionary<string, string> buffers;
        // port -> values
        private static ConcurrentDictionary<string, List<int>> lastValues;
        // port -> lastUpdate
        private static ConcurrentDictionary<string, MonotonicTimestamp> lastUpdates;
        // port -> SerialPort
        private static ConcurrentDictionary<string, SerialPort> serialPorts;

        static DeejIn()
        {
            callbacks = new ConcurrentDictionary<string, List<Action<List<int>>>>();
            generalCallbacks = new List<Action<string, List<int>>>();

            watchedDevices = new List<string>();
            buffers = new ConcurrentDictionary<string, string>();
            
            lastValues = new ConcurrentDictionary<string, List<int>>();
            lastUpdates = new ConcurrentDictionary<string, MonotonicTimestamp>();
            serialPorts = new ConcurrentDictionary<string, SerialPort>();
            
            deviceWatcher = DeviceInformation.CreateWatcher(SerialDevice.GetDeviceSelector());
            deviceWatcher.Added += Added;
            deviceWatcher.Removed += Removed;

            deviceWatcher.Start();
        }
        
        public static void AddCallback(string port, Action<List<int>> callback)
        {
            if (!callbacks.ContainsKey(port) || callbacks[port].Count == 0)
            {
                callbacks[port] = new List<Action<List<int>>>();
                _StartListening(port);
            }

            callbacks[port].Add(callback);
        }

        public static void RemoveCallback(string port, Action<List<int>> callback)
        {
            if (port == null)
            {
                return;
            }
            
            if (callbacks.ContainsKey(port))
            {
                if (callbacks[port].Contains(callback))
                {
                    callbacks[port].Remove(callback);
                }

                // Stop listening if there are no more events on the device
                if (callbacks[port].Count == 0)
                {
                    try
                    {
                        serialPorts[port].Close();
                    }
                    catch (IOException)
                    {
                        
                    }
                    
                    watchedDevices.Remove(port);
                }
            }
        }
        
        public static List<string> GetAllDevices()
        {
            return new List<string>(SerialPort.GetPortNames());
        }

        internal static void _StartListening(string port)
        {
            if (watchedDevices.Contains(port) || !GetAllDevices().Contains(port))
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
                buffers[port] = "";
            }

            if (!lastValues.ContainsKey(port))
            {
                lastValues[port] = new List<int>();
            }
            
            try
            {
                sp.Open();
                watchedDevices.Add(port);
                serialPorts[port] = sp;
            }
            catch (IOException)
            {

            }
            catch (UnauthorizedAccessException)
            {
                
            }
        }

        internal static void AddGeneralCallback(Action<string, List<int>> callback)
        {
            generalCallbacks.Add(callback);
        }
        
        private static bool SendCallback(string port, List<int> values)
        {
            if (!lastUpdates.ContainsKey(port))
            {
                lastUpdates[port] = MonotonicTimestamp.Now();
            }

            if ((MonotonicTimestamp.Now() - lastUpdates[port]).Milliseconds > 100)
            {
                lastValues[port] = values;
                lastUpdates[port]= MonotonicTimestamp.Now();
                return true;
            }
            
            if (lastValues[port].Count != values.Count)
            {
                lastValues[port] = values;
                lastUpdates[port]= MonotonicTimestamp.Now();
                return true;
            }

            if (values.Where((t, i) => t != lastValues[port][i]).Any())
            {
                lastValues[port] = values;
                lastUpdates[port]= MonotonicTimestamp.Now();
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

            foreach (var port in serialPorts.Keys)
            {
                var serial = serialPorts[port];
                if (!serial.IsOpen && watchedDevices.Contains(serial.PortName))
                {
                    watchedDevices.Remove(serial.PortName);
                }
            }
        }
    }
}