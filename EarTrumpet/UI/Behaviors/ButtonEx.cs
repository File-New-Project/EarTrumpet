using EarTrumpet.Extensions;
using EarTrumpet.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace EarTrumpet.UI.Behaviors
{
    public class ButtonEx
    {
        public enum ClickActionKind
        {
            None,
            Maximize,
            Minimize,
            Close
        }

        // ClickAction: Caption button commands.
        public static ClickActionKind GetClickAction(DependencyObject obj) => (ClickActionKind)obj.GetValue(ClickActionProperty);
        public static void SetClickAction(DependencyObject obj, ClickActionKind value) => obj.SetValue(ClickActionProperty, value);
        public static readonly DependencyProperty ClickActionProperty =
        DependencyProperty.RegisterAttached("ClickAction", typeof(ClickActionKind), typeof(ButtonEx), new PropertyMetadata(ClickActionKind.None, ClickActionChanged));
        private static void ClickActionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var btn = (Button)dependencyObject;
            var kind = (ClickActionKind)e.NewValue;
            btn.Click += (_, __) =>
            {
                Debug.Assert(kind == (ClickActionKind)e.NewValue);
                switch (kind)
                {
                    case ClickActionKind.Close:
                        Window.GetWindow(btn)?.Close();
                        break;
                    case ClickActionKind.Minimize:
                    case ClickActionKind.Maximize:
                        var window = Window.GetWindow(btn);
                        if (window != null)
                        {
                            window.WindowState = kind == ClickActionKind.Minimize ? WindowState.Minimized :
                                        (window.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
                        }
                        break;
                    default: throw new NotImplementedException();
                }
            };
        }

        // ClickPopup: Attach Popup to Button.Click.
        public static Popup GetClickPopup(DependencyObject obj) => (Popup)obj.GetValue(ClickPopupProperty);
        public static void SetClickPopup(DependencyObject obj, Popup value) => obj.SetValue(ClickPopupProperty, value);
        public static readonly DependencyProperty ClickPopupProperty =
        DependencyProperty.RegisterAttached("ClickPopup", typeof(Popup), typeof(ButtonEx), new PropertyMetadata(null, ClickPopupChanged));
        private static void ClickPopupChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var popup = (Popup)e.NewValue;
            var btn = (Button)dependencyObject;

            btn.Click += (_, __) =>
            {
                var dpiX = Window.GetWindow(btn).DpiX();
                var dpiY = Window.GetWindow(btn).DpiY();

                popup.Opacity = 0;
                popup.DataContext = btn.DataContext;
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
        
        // IsToolBarButton: Fix up some issues with ContextMenu binding.
        public static bool GetIsToolBarButton(DependencyObject obj) => (bool)obj.GetValue(IsToolBarButtonProperty);
        public static void SetIsToolBarButton(DependencyObject obj, bool value) => obj.SetValue(IsToolBarButtonProperty, value);
        public static readonly DependencyProperty IsToolBarButtonProperty =
        DependencyProperty.RegisterAttached("IsToolBarButton", typeof(bool), typeof(ButtonEx), new PropertyMetadata(false, IsToolBarButtonChanged));
        private static void IsToolBarButtonChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert((bool)e.NewValue);

            var btn = (Button)dependencyObject;
            btn.Click += (_, __) =>
            {
                var dt = (ToolbarItemViewModel)btn.DataContext;
                if (dt.Menu != null)
                {
                    btn.ContextMenu.Opened += (___, _____) =>
                    {
                        // Remove exit animation.
                        ((Popup)btn.ContextMenu.Parent).PopupAnimation = PopupAnimation.None;
                    };
                    btn.ContextMenu.PlacementTarget = btn;
                    btn.ContextMenu.Placement = PlacementMode.Bottom;
                    btn.ContextMenu.IsOpen = true;
                }
            };
            btn.PreviewMouseRightButtonDown += (_, __) =>
            {
                var dt = (ToolbarItemViewModel)btn.DataContext;
                if (dt.Menu == null)
                {
                    btn.ContextMenu = null;
                }
            };
        }
    }
}
