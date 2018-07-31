using EarTrumpet.DataModel;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace EarTrumpet_Actions.DataModel
{
    public class App
    {
        public static readonly string EveryAppId = "EarTrumpet.EveryApp";
        public static readonly string ForegroundAppId = "EarTrumpet.ForegroundApp";
        
        [Flags]
        public enum AppKind
        {
            Default = 0,
            EveryApp = 1,
            ForegroundApp = 2,
        }

        public string Id { get; set; }

        public static ObservableCollection<Option> GetApps(AppKind flags)
        {
            var ret = new ObservableCollection<Option>();
            if ((flags & AppKind.EveryApp) == AppKind.EveryApp)
            {
                ret.Add(new Option(Properties.Resources.EveryAppText, new App { }));
            }
            if ((flags & AppKind.ForegroundApp) == AppKind.ForegroundApp)
            {
                ret.Add(new Option(Properties.Resources.ForegroundAppText, new App { Id = ForegroundAppId }));
            }

            foreach (var device in DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback).Devices.SelectMany(d => d.Groups))
            {
                ret.Add(new Option(device.SessionDisplayName, new App(device)));
            }
            return ret;
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

            if (Id == EveryAppId)
            {
                return Properties.Resources.EveryAppText;
            }
            if (Id == ForegroundAppId)
            {
                return Properties.Resources.ForegroundAppText;
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