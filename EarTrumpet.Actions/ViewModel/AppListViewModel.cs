using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace EarTrumpet_Actions.ViewModel
{
    class AppListViewModel : BindableBase
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

        public ObservableCollection<AppViewModelBase> All { get; }

        private IPartWithApp _part;

        public AppListViewModel(IPartWithApp part, AppKind flags)
        {
            _part = part;
            All = new ObservableCollection<AppViewModelBase>();

            GetApps(flags);

            if (part.App?.Id == null)
            {
                _part.App = new App { Id = All[0].Id };
            }
        }

        public void OnInvoked(object sender, AppViewModelBase vivewModel)
        {
            _part.App = new App { Id = vivewModel.Id };
            RaisePropertyChanged("");  // Signal change so ToString will be called.

            var popup = ((DependencyObject)sender).FindVisualParent<Popup>();
            popup.IsOpen = false;
        }

        public override string ToString()
        {
            var existing = All.FirstOrDefault(d => d.Id == _part.App?.Id);
            if (existing != null)
            {
                return existing.DisplayName;
            }
            return _part.App?.Id;
        }

        public void GetApps(AppKind flags)
        {
            if ((flags & AppKind.EveryApp) == AppKind.EveryApp)
            {
                All.Add(new EveryAppViewModel());
            }

            if ((flags & AppKind.ForegroundApp) == AppKind.ForegroundApp)
            {
                All.Add(new ForegroundAppViewModel());
            }

            foreach (var app in DataModelFactory.CreateAudioDeviceManager(AudioDeviceKind.Playback).Devices.SelectMany(d => d.Groups).Distinct(IAudioDeviceSessionComparer.Instance).OrderBy(d => d.SessionDisplayName))
            {
                All.Add(new AppViewModel(app));
            }
        }
    }
}