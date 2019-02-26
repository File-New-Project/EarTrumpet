using EarTrumpet.Extensions;
using EarTrumpet.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace EarTrumpet.UI.Controls
{
    public partial class SearchBox : TextBox
    {
        public object ItemsSource
        {
            get { return (object)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
          "ItemsSource", typeof(object), typeof(SearchBox), new PropertyMetadata(null));

        private bool _ignoreNextFocus;

        public SearchBox()
        {
            InitializeComponent();
            TextChanged += SearchBox_TextChanged;

            GotKeyboardFocus += SearchBox_GotKeyboardFocus;
            PreviewKeyUp += SearchBox_PreviewKeyUp;
        }

        private void SearchBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Down)
            {
                var popup = ((Popup)GetTemplateChild("Popup"));
                if (popup.IsOpen)
                {
           
                        Keyboard.Focus(popup.Child.FindVisualChild<Control>());
            
                }
            }
        }

        private void SearchBox_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            MaybeShowSearchPopup();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBlock)GetTemplateChild("PromptText")).Visibility = Text.Length > 0 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

            MaybeShowSearchPopup();
        }

        private void MaybeShowSearchPopup()
        {
            if (_ignoreNextFocus)
            {
                _ignoreNextFocus = false;
                return;
            }

            if (Text.Length > 0)
            {
                var popup = ((Popup)GetTemplateChild("Popup"));
                var vm = new SettingsSearchBoxResultsViewModel((SettingsViewModel)ItemsSource, Text, () =>
                {
                    _ignoreNextFocus = true;
                    popup.IsOpen = false;
                    Focus();
                    Text = "";
                });

                popup.PlacementTarget = this;
                popup.Width = ActualWidth;
                popup.DataContext = vm;
                popup.UpdateLayout();
                popup.IsOpen = true;
            }
        }
    }
}
