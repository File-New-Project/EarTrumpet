using System;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace EarTrumpet.UWP
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                ShowMainPage();
            }
        }

        private void ShowMainPage()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Colors.Transparent;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 450)); // min 192x48 max 500x500
            ApplicationView.PreferredLaunchViewSize = new Size(500, 450); //min 500x320
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;                    
            rootFrame.Navigate(typeof(WelcomePage));

            Window.Current.Activate();

            UISettings uiSettings = new UISettings();
            uiSettings.ColorValuesChanged += UiSettings_ColorValuesChanged;
            UiSettings_ColorValuesChanged(uiSettings, null);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {            
            ShowMainPage();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private async void UiSettings_ColorValuesChanged(UISettings sender, object args)
        {
            var backgroundColor = sender.GetColorValue(UIColorType.Background);
            var isDarkMode = backgroundColor == Colors.Black;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

                if (isDarkMode)
                {
                    titleBar.ButtonForegroundColor = Colors.White;
                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(51, 255, 255, 255);
                }
                else
                {
                    titleBar.ButtonForegroundColor = Colors.Black;
                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(51, 0, 0, 0);
                }
            });
        }
    }
}
