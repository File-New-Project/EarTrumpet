using EarTrumpet.Extensions;
using EarTrumpet.Models;
using EarTrumpet.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace EarTrumpet.ViewModels
{
    public class AudioMixerViewModel : BindableBase
    {
        // This encapsulates the functionality used for AppItemViewModels to call back to AudioMixerViewModel
        // and thus interact with the audio session service.
        public class AudioMixerViewModelCallbackProxy : IAudioMixerViewModelCallback
        {
            private readonly EarTrumpetAudioSessionService _service;   
            public AudioMixerViewModelCallbackProxy(EarTrumpetAudioSessionService service)
            {
                _service = service;
            }

            // IAudioMixerViewModelCallback
            public void SetVolume(EarTrumpetAudioSessionModel item, float volume)
            {
                _service.SetAudioSessionVolume(item.SessionId, volume);
            }
        }

        public ObservableCollection<AppItemViewModel> Apps { get; private set; }

        public Visibility ListVisibility { get; private set; }
        public Visibility NoAppsPaneVisibility { get; private set; }

        private readonly EarTrumpetAudioSessionService _audioService;
        private readonly AudioMixerViewModelCallbackProxy _proxy;

        public AudioMixerViewModel()
        {
            Apps = new ObservableCollection<AppItemViewModel>();
            _audioService = new EarTrumpetAudioSessionService();
            _proxy = new AudioMixerViewModelCallbackProxy(_audioService);
        }

        public void Refresh()
        {
            bool hasApps = Apps.Count > 0;

            var titleProvider = ProcessTitleProviderFactoryService.CreateProvider();
            var sessions = _audioService.GetAudioSessionGroups().Select(x => new AppItemViewModel(_proxy, x, titleProvider));

            List<AppItemViewModel> staleSessionsToRemove = new List<AppItemViewModel>();

            // remove stale apps
            foreach (var app in Apps)
            {
                if (!sessions.Where(x => (x.IsSame(app) && (!app.IsDesktop || UserPreferencesService.ShowDesktopApps))).Any())
                {
                    staleSessionsToRemove.Add(app);
                }
            }
            foreach (var app in staleSessionsToRemove) { Apps.Remove(app); }

            // add new apps
            foreach (var session in sessions)
            {
                var findApp = Apps.FirstOrDefault(x => x.IsSame(session));
                if (findApp == null)
                {
                    if (!session.IsDesktop || UserPreferencesService.ShowDesktopApps)
                    {
                        Apps.AddSorted(session, AppItemViewModelComparer.Instance);
                    }
                }
                else
                {
                    // update existing apps
                    findApp.UpdateFromOther(session);
                }
            }

            ListVisibility = Apps.Count > 0 ? Visibility.Visible : Visibility.Hidden;
            NoAppsPaneVisibility = Apps.Count == 0 ? Visibility.Visible : Visibility.Hidden;

            if (hasApps != (Apps.Count > 0))
            {
                RaisePropertyChanged("ListVisibility");
                RaisePropertyChanged("NoAppsPaneVisibility");
            }
        }
    }
}
