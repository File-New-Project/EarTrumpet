using EarTrumpet.DataModel;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.ViewModels;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();

            SourceInitialized += (_, __) => AccentPolicyLibrary.SetWindowBlur(this, true, true);

            this.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            DataContextChanged += DialogWindow_DataContextChanged;
        }

        private void DialogWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is IWindowHostedViewModel)
            {
                var vm = (IWindowHostedViewModel)e.NewValue;
                vm.Close += () => Close();
                vm.HostDialog += (d) =>
                {
                    var dialog = new DialogWindow { Owner = this };
                    dialog.DataContext = d;
                    dialog.ShowDialog();
                };
                Closing += (_, __) => vm.OnClosing();
                PreviewKeyDown += (_, eKey) => vm.OnPreviewKeyDown(eKey);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
