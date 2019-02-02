using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using System;
using System.Windows.Input;
using Windows.ApplicationModel;

namespace EarTrumpet.UI.ViewModels
{
    public class EarTrumpetLegacySettingsPageViewModel : SettingsPageViewModel, IWindowHostedViewModel
    {
        private HotkeyData _hotkey;

        internal HotkeyData Hotkey
        {
            get => _hotkey;
            set
            {
                _hotkey = value;
                SettingsService.Hotkey = _hotkey;
                RaisePropertyChanged(nameof(Hotkey));
                RaisePropertyChanged(nameof(HotkeyText));
            }
        }

#pragma warning disable CS0067
        public event Action Close;
#pragma warning restore CS0067
        public event Action<object> HostDialog;

        public string HotkeyText => _hotkey.ToString();
        public string DefaultHotKey => SettingsService.s_defaultHotkey.ToString();

        public RelayCommand SelectHotkey { get; }
        public RelayCommand OpenAddonManager { get; set; }

        public bool IsAddonsEnabled => Features.IsEnabled(Feature.Addons);
        public bool UseLegacyIcon
        {
            get => SettingsService.UseLegacyIcon;
            set => SettingsService.UseLegacyIcon = value;
        }


        public EarTrumpetLegacySettingsPageViewModel() : base(null)
        {
            Title = Properties.Resources.KeyboardShortcutsTitle;
            Glyph = "\xE713";
            Hotkey = SettingsService.Hotkey;

            SelectHotkey = new RelayCommand(OnSelectHotkey);


        }

        private void OnSelectHotkey()
        {
            var vm = new HotkeySelectViewModel();
            HostDialog.Invoke(vm);
            if (vm.Saved)
            {
                HotkeyManager.Current.Unregister(Hotkey);
                Hotkey = vm.Hotkey;
                HotkeyManager.Current.Register(Hotkey);
            }
        }

        public void OnClosing()
        {

        }

        public void OnPreviewKeyDown(KeyEventArgs e)
        {

        }
    }
}