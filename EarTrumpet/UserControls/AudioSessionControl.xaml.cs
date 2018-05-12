using EarTrumpet.Extensions;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace EarTrumpet.UserControls
{
    public partial class AudioSessionControl : UserControl
    {
        public AudioSessionViewModel Stream { get { return (AudioSessionViewModel)GetValue(StreamProperty); } set { SetValue(StreamProperty, value); } }
        public static readonly DependencyProperty StreamProperty = DependencyProperty.Register(
          "Stream", typeof(AudioSessionViewModel), typeof(AudioSessionControl), new PropertyMetadata(null, new PropertyChangedCallback(StreamChanged)));

        public ImageSource IconSource { get { return (ImageSource)GetValue(IconUriProperty); } set { SetValue(IconUriProperty, value); } }
        public static readonly DependencyProperty IconUriProperty =
            DependencyProperty.Register("IconSource", typeof(ImageSource), typeof(AudioSessionControl), new PropertyMetadata(null));


        public string IconText { get { return (string)GetValue(IconTextProperty); } set { SetValue(IconTextProperty, value); } }
        public static readonly DependencyProperty IconTextProperty =
            DependencyProperty.Register("IconText", typeof(string), typeof(AudioSessionControl), new PropertyMetadata(""));

        public Brush IconBackground { get { return (Brush)GetValue(IconBackgroundProperty); } set { SetValue(IconBackgroundProperty, value); } }
        public static readonly DependencyProperty IconBackgroundProperty =
            DependencyProperty.Register("IconBackground", typeof(Brush), typeof(AudioSessionControl), new PropertyMetadata(null));

        public AudioSessionControl()
        {
            InitializeComponent();
            GridRoot.DataContext = this;
        }


        private void Mute_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Stream.IsMuted = !Stream.IsMuted;
                e.Handled = true;
            }
        }

        private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Stream.IsMuted = !Stream.IsMuted;
                e.Handled = true;
            }
        }

        private static void StreamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AudioSessionControl)d).StreamChanged();
        }

        private void StreamChanged()
        {

        }

        private void UserControl_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var app = Stream as AppItemViewModel;
            if (app != null && !app.IsExpanded)
            {
                app.IsExpanded = true;
                var oldApp = MainViewModel.ExpandedApp;
                MainViewModel.ExpandedApp = app;

                if (oldApp != null && oldApp != app)
                {
                    oldApp.IsExpanded = false;
                }
            }
        }
    }
}
