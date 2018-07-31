using EarTrumpet.DataModel;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace EarTrumpet_Actions.DataModel
{
    public class App : IEquatable<App>
    {
        public string Id { get; set; }

        public static ObservableCollection<Option> AllApps
        {
            get
            {
                var ret = new ObservableCollection<Option>();
                ret.Add(new Option("Choose an app", new App { }));
                foreach (var device in DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback).Devices.SelectMany(d => d.Groups))
                {
                    ret.Add(new Option(device.SessionDisplayName, new App(device)));
                }
                return ret;
            }
        }

        public App()
        {

        }

        public App(IAudioDeviceSession device)
        {
            Id = device.AppId;
        }

        public override string ToString()
        {
            var appSession = DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback).Devices.SelectMany(c => c.Groups).FirstOrDefault(d => d.AppId == Id);
            if (appSession != null)
            {
                return appSession.SessionDisplayName;
            }

            if (Id == null)
            {
                return "(Choose an app)";
            }

            return Path.GetFileName(Id);
        }

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.GetHashCode();
        }

        public bool Equals(App other)
        {
            return other.Id == Id;
        }
    }
}