using EarTrumpet.Extensions;
using EarTrumpet.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EarTrumpet.UI.Views
{
    public partial class AppItemView : UserControl
    {
        private IAppItemViewModel App => (IAppItemViewModel)DataContext;

        public AppItemView()
        {
            InitializeComponent();

            PreviewMouseRightButtonUp += (_, __) => OpenPopup();
            Loaded += (_, __) =>
            {
                var container = this.FindVisualParent<ListViewItem>();
                if (container != null)
                {
                    container.PreviewKeyDown += OnPreviewKeyDown;
                }
            };
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.M:
                case Key.OemPeriod:
                    App.IsMuted = !App.IsMuted;
                    e.Handled = true;
                    break;
                case Key.Right:
                case Key.OemPlus:
                    App.Volume++;
                    e.Handled = true;
                    break;
                case Key.Left:
                case Key.OemMinus:
                    App.Volume--;
                    e.Handled = true;
                    break;
                case Key.Space:
                    OpenPopup();
                    e.Handled = true;
                    break;
            }
        }

        private void OpenPopup()
        {
            var viewModel = Window.GetWindow(this).DataContext as IPopupHostViewModel;
            if (viewModel != null && App != null && !App.IsExpanded)
            {
                viewModel.OpenPopup(App, this);
            }
        }
    }
}
