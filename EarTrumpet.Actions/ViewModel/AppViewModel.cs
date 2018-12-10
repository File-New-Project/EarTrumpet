using EarTrumpet.DataModel;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet_Actions.ViewModel
{
    class AppViewModel : BindableBase, IOptionViewModel
    {
        [Flags]
        public enum AppKind
        {
            Default = 0,
            EveryApp = 1,
            ForegroundApp = 2,
        }

        class IAudioDeviceSessionComparer : IEqualityComparer<IAudioDeviceSession>
        {
            public static readonly IAudioDeviceSessionComparer Instance = new IAudioDeviceSessionComparer();

            public bool Equals(IAudioDeviceSession x, IAudioDeviceSession y)
            {
                return x.AppId.Equals(y.AppId);
            }

            public int GetHashCode(IAudioDeviceSession obj)
            {
                return obj.AppId.GetHashCode();
            }
        }

        public ObservableCollection<Option> All { get; }

        public Option Selected
        {
            get => All.FirstOrDefault(d => ((App)d.Value)?.Id == _part.App?.Id);
            set
            {
                if (Selected != value)
                {
                    _part.App = (App)value.Value;
                    RaisePropertyChanged(nameof(Selected));
                }
            }
        }

        private IPartWithApp _part;

        public AppViewModel(IPartWithApp part, AppKind flags)
        {
            _part = part;
            All = GetApps(flags);
            if (Selected == null && _part.App?.Id != null)
            {
                All.Add(new Option(_part.App.Id, _part.App));
            }

            if (_part.App?.Id == null)
            {
                _part.App = (App)All[0].Value;
            }
        }

        public override string ToString()
        {
            return Selected?.DisplayName;
        }

        public static ObservableCollection<Option> GetApps(AppKind flags)
        {
            var ret = new ObservableCollection<Option>();
            if ((flags & AppKind.EveryApp) == AppKind.EveryApp)
            {
                ret.Add(new Option(Properties.Resources.EveryAppText, new App { Id = App.EveryAppId }));
            }

            if ((flags & AppKind.ForegroundApp) == AppKind.ForegroundApp)
            {
                ret.Add(new Option(Properties.Resources.ForegroundAppText, new App { Id = App.ForegroundAppId }));
            }

            foreach (var app in DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback).Devices.SelectMany(d => d.Groups).Distinct(IAudioDeviceSessionComparer.Instance).OrderBy(d => d.SessionDisplayName))
            {
                ret.Add(new Option(app.SessionDisplayName, new App { Id = app.AppId }));
            }
            return ret;
        }
    }
}