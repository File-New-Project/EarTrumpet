using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.Services;
using System;
using System.Windows.Input;
using Windows.ApplicationModel;

namespace EarTrumpet.UI.ViewModels
{
    public class SettingsViewModel : BindableBase, IWindowHostedViewModel
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

        public string Title => Properties.Resources.SettingsWindowText;
        public string HotkeyText => _hotkey.ToString();
        public string DefaultHotKey => SettingsService.s_defaultHotkey.ToString();
        public RelayCommand OpenDiagnosticsCommand { get; }
        public RelayCommand OpenAboutCommand { get; }
        public RelayCommand OpenFeedbackCommand { get; }
        public RelayCommand SelectHotkey { get; }
        public RelayCommand OpenAddonManager { get; set; }

        public bool IsAddonsEnabled => Features.IsEnabled(Feature.Addons);
        public bool UseLegacyIcon
        {
            get => SettingsService.UseLegacyIcon;
            set => SettingsService.UseLegacyIcon = value;
        }

        public bool UseLogarithmicVolume
        {
            get => SettingsService.UseLogarithmicVolume;
            set => SettingsService.UseLogarithmicVolume = value;
        }

        public string AboutText { get; private set; }

        internal SettingsViewModel()
        {
            Hotkey = SettingsService.Hotkey;
            OpenAboutCommand = new RelayCommand(OpenAbout);
            OpenDiagnosticsCommand = new RelayCommand(OpenDiagnostics);
            OpenFeedbackCommand = new RelayCommand(FeedbackService.OpenFeedbackHub);
            SelectHotkey = new RelayCommand(OnSelectHotkey);

            string aboutFormat = "EarTrumpet {0}";
            if (App.Current.HasIdentity())
            {
                AboutText = string.Format(aboutFormat, Package.Current.Id.Version.ToVersionString());
            }
            else
            {
                AboutText = string.Format(aboutFormat, "0.0.0.0");
            }
        }

        private void OpenDiagnostics()
        {
            if(Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                throw new Exception("This is an intentional crash.");
            }

            DiagnosticsService.DumpAndShowData();
        }

        private void OpenAbout()
        {
            ProcessHelper.StartNoThrow("https://github.com/File-New-Project/EarTrumpet");
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