using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class SettingsWindow : Window
    {
        private bool _isClosing;

        public SettingsWindow()
        {
            Trace.WriteLine("SettingsWindow .ctor");

            InitializeComponent();

            SourceInitialized += SettingsWindow_SourceInitialized;

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            DataContextChanged += SettingsWindow_DataContextChanged;
        }

        private void SettingsWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is IWindowHostedViewModel)
            {
                var vm = (IWindowHostedViewModel)e.NewValue;
                vm.Close += () => Close();
                vm.HostDialog += (dialogDataContext) =>
                {
                    var dialog = new DialogWindow { Owner = this };
                    dialog.DataContext = dialogDataContext;
                    dialog.ShowDialog();
                };
                Closing += (_, __) => vm.OnClosing();
                PreviewKeyDown += (_, eKey) => vm.OnPreviewKeyDown(eKey);
            }
        }

        private void SettingsWindow_SourceInitialized(object sender, System.EventArgs e)
        {
            Trace.WriteLine("SettingsWindow SettingsWindow_SourceInitialized");

            this.Cloak();
            AccentPolicyLibrary.SetWindowBlur(this, true, true);
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("SettingsWindow CloseButton_Click");

            if (!_isClosing)
            {
                // Ensure we don't double-animate if the user is able to close us multiple ways before the window stops accepting input.
                _isClosing = true;
                WindowAnimationLibrary.BeginWindowExitAnimation(this, () => this.Close());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ((SettingsViewModel)DataContext).Selected = null;
        }
    }
}
