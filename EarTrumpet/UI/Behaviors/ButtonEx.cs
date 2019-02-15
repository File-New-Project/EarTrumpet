using EarTrumpet.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace EarTrumpet.UI.Behaviors
{
    public class ButtonEx
    {
        public static Popup GetClickPopup(DependencyObject obj) => (Popup)obj.GetValue(ClickPopupProperty);
        public static void SetClickPopup(DependencyObject obj, Popup value) => obj.SetValue(ClickPopupProperty, value);
        public static readonly DependencyProperty ClickPopupProperty =
        DependencyProperty.RegisterAttached("ClickPopup", typeof(Popup), typeof(Popup), new PropertyMetadata(null, PopupChanged));
        private static void PopupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var popup = (Popup)e.NewValue;
            var btn = (Button)d;

            btn.Click += (_, __) =>
            {
                var dpiX = Window.GetWindow(btn).DpiWidthFactor();
                var dpiY = Window.GetWindow(btn).DpiHeightFactor();

                popup.Opacity = 0;
                popup.UpdateLayout();
                popup.Child.UpdateLayout();
                popup.IsOpen = true;
                popup.Dispatcher.BeginInvoke((Action)(() =>
                {
                    popup.Opacity = 1;
                    popup.HorizontalOffset = -1 * (popup.Child.RenderSize.Width / dpiX) / 2;
                    popup.VerticalOffset = -1 * (popup.Child.RenderSize.Height / dpiY) / 2;
                    Keyboard.Focus(popup.Child.FindVisualChild<Control>());
                }),
                System.Windows.Threading.DispatcherPriority.DataBind, null);
            };
        }
    }
}
