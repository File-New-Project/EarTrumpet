using EarTrumpet.DataModel;
using EarTrumpet.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EarTrumpet.ViewModels.Mocks
{
    public class SettingsViewModelMock
    {
        class MockAudioSession : BindableBase, IStreamWithVolumeControl
        {
            public string DisplayName { get; set; }

            public string Id { get; set; }

            public bool IsMuted { get; set; }
            public float Volume { get; set; }

            public float PeakValue { get; set; }
        }


        public SettingsViewModelMock()
        {
            DefaultApps = new ObservableCollection<AudioSessionViewModel>();

            DefaultApps.Add(new AudioSessionViewModel(new MockAudioSession { DisplayName = "Test 1" }));
            DefaultApps.Add(new AudioSessionViewModel(new MockAudioSession { DisplayName = "Test 2" }));

            Hotkey = new SettingsService.HotkeyData { Key = System.Windows.Forms.Keys.A, Modifiers = KBModifierKeys.Control | KBModifierKeys.Shift };
        }

        public ObservableCollection<AudioSessionViewModel> DefaultApps { get; private set; }

        public SettingsService.HotkeyData Hotkey { get; private set; }
        public string HotkeyText => Hotkey.ToString();
    }
}
