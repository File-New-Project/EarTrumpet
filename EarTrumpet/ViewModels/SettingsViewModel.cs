using EarTrumpet.DataModel;
using EarTrumpet.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        IAudioDeviceManager _manager;

        public ObservableCollection<AudioSessionViewModel> DefaultApps { get; private set; }

        SettingsService.HotkeyData _hotkey;
        public SettingsService.HotkeyData Hotkey
        {
            get => _hotkey;
            set
            {
                _hotkey = value;
                RaisePropertyChanged(nameof(Hotkey));
            }
        }

        public SettingsViewModel(IAudioDeviceManager manager)
        {
            _manager = manager;

            DefaultApps = new ObservableCollection<AudioSessionViewModel>();

            foreach(var app in SettingsService.DefaultApps)
            {
                DefaultApps.Add(new AppItemViewModel(new SettingAudioSession(app, manager)));
            }

            Hotkey = SettingsService.Hotkey;
        }

        public IEnumerable<AppItemViewModel> EnumerateApps()
        {
            if (!_manager.VirtualDefaultDevice.IsDevicePresent)
            {
                yield break;
            }

            foreach(var app in _manager.DefaultPlaybackDevice.Sessions)
            {
                if (!DefaultApps.Any(d => AppResolverService.GetAppIdForProcess((uint)app.ProcessId) == d.Id))
                {
                    yield return new AppItemViewModel(app);
                }
            }
        }

        internal void AddApp(IAudioDeviceSession app)
        {
            DefaultApps.Add(new AppItemViewModel(new SettingAudioSession(new SettingsService.DefaultApp
            { Id = AppResolverService.GetAppIdForProcess((uint)app.ProcessId) }, _manager)));
        }

        public void Save()
        {
            SettingsService.DefaultApps = DefaultApps.Select(d => ((SettingAudioSession)((AppItemViewModel)d).Session).Data).ToArray();
            SettingsService.Hotkey = Hotkey;
            HotkeyService.Register(Hotkey.Modifiers, Hotkey.Key);
        }
    }
}
