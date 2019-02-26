using EarTrumpet.Extensions;
using EarTrumpet.UI.ViewModels;
using EarTrumpet.Actions.DataModel;
using EarTrumpet.Actions.DataModel.Serialization;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using EarTrumpet.DataModel.WindowsAudio;
using EarTrumpet.DataModel.Audio;

namespace EarTrumpet.Actions.ViewModel
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

        public ObservableCollection<IAppItemViewModel> All { get; }

        private IPartWithApp _part;

        public AppListViewModel(IPartWithApp part, AppKind flags)
        {
            _part = part;
            All = new ObservableCollection<IAppItemViewModel>();

            GetApps(flags);

            if (part.App?.Id == null)
            {
                _part.App = new AppRef { Id = All[0].Id };
            }
        }

        public void OnInvoked(object sender, IAppItemViewModel vivewModel)
        {
            _part.App = new AppRef { Id = vivewModel.Id };
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

            foreach (var app in WindowsAudioFactory.Create(AudioDeviceKind.Playback).Devices.SelectMany(d => d.Groups).Distinct(IAudioDeviceSessionComparer.Instance).OrderBy(d => d.DisplayName).OrderBy(d => d.DisplayName))
            {
                All.Add(new SettingsAppItemViewModel(app));
            }
        }
    }
}