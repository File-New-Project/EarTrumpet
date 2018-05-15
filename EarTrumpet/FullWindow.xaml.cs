using EarTrumpet.UserControls;
using EarTrumpet.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet
{
    public partial class FullWindow : Window
    {
        public static FullWindow Instance { get; private set; }

        private FullWindowViewModel _viewModel;
        private VolumeControlPopup _popup;

        public FullWindow(MainViewModel viewModel)
        {
            Instance = this;

            _viewModel = new FullWindowViewModel(viewModel);
            _viewModel.AppExpanded += OnAppExpanded;
            _viewModel.AppCollapsed += OnAppCollapsed;

            InitializeComponent();

            Title = Properties.Resources.FullWindowTitleText;
            _popup = (VolumeControlPopup)Resources["AppPopup"];
            _popup.Closed += (_, __) => _viewModel.OnAppCollapsed();

            LocationChanged += (_, __) => _viewModel.OnAppCollapsed();
            SizeChanged += (_, __) => _viewModel.OnAppCollapsed();

            DataContext = _viewModel;

            this.StateChanged += FullWindow_StateChanged;

            Activated += (_, __) => SizeToContent = SizeToContent.Manual;
        }

        private void OnAppCollapsed(object sender, object e)
        {
            ContentRoot.Children.Remove(_popup);
            _popup.IsOpen = false;
        }

        private void OnAppExpanded(object sender, AppExpandedEventArgs e)
        {
            var selectedApp = e.ViewModel;

            _popup.DataContext = selectedApp;
            ContentRoot.Children.Add(_popup);

            Point relativeLocation = e.Container.TranslatePoint(new Point(0, 0), this);

            double HEADER_SIZE = (double)App.Current.Resources["DeviceTitleCellHeight"];
            double ITEM_SIZE = (double)App.Current.Resources["AppItemCellHeight"];
            Thickness volumeListMargin = (Thickness)App.Current.Resources["VolumeAppListMargin"];

            // TODO: can't figure out where this 6px is from
            relativeLocation.Y -= HEADER_SIZE + 6;

            var popupHeight = HEADER_SIZE + (selectedApp.ChildApps.Count * ITEM_SIZE) + volumeListMargin.Bottom + volumeListMargin.Top;

            // TODO: Cap top as well as bottom
            if (relativeLocation.Y + popupHeight > ActualHeight)
            {
                relativeLocation.Y = ActualHeight - popupHeight;
            }

            _popup.Placement = System.Windows.Controls.Primitives.PlacementMode.Absolute;
            _popup.HorizontalOffset = this.PointToScreen(new Point(0, 0)).X + relativeLocation.X;
            _popup.VerticalOffset = this.PointToScreen(new Point(0, 0)).Y + relativeLocation.Y;

            _popup.Width = ((FrameworkElement)e.Container).ActualWidth;
            _popup.Height = popupHeight;

            _popup.ShowWithAnimation();
        }

        private void FullWindow_StateChanged(object sender, System.EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }

            _viewModel.OnAppCollapsed();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            _viewModel.Close();
        }

        private void DeviceAndAppsControl_AppExpanded(object sender, UserControls.AppVolumeControlExpandedEventArgs e)
        {
            _viewModel.OnAppExpanded(e.ViewModel, e.Container);
        }

        private void LightDismissBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _popup.HideWithAnimation();
            e.Handled = true;
        }
    }
}
