using EarTrumpet.Extensions;
using EarTrumpet.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EarTrumpet.Views
{
    public class KeyboardNavigator
    {
        public static void OnKeyDown(FrameworkElement host, ref KeyEventArgs evt)
        {
            var focused = FocusManager.GetFocusedElement(host);
            var listItem = focused as ListViewItem;

            if (listItem != null)
            {
                var app = listItem.DataContext as IAppItemViewModel;

                if (app != null)
                {
                    switch (evt.Key)
                    {
                        case Key.M:
                        case Key.OemPeriod:
                            app.IsMuted = !app.IsMuted;
                            evt.Handled = true;
                            break;
                        case Key.Right:
                        case Key.OemPlus:
                            app.Volume++;
                            evt.Handled = true;
                            break;
                        case Key.Left:
                        case Key.OemMinus:
                            app.Volume--;
                            evt.Handled = true;
                            break;
                        case Key.Space:
                            var volControl = listItem.FindVisualChild<AppVolumeControl>();
                            volControl.ExpandApp();
                            evt.Handled = true;
                            break;
                    }
                }
                else
                {
                    var device = (DeviceViewModel)listItem.DataContext;
                    switch (evt.Key)
                    {
                        case Key.M:
                        case Key.OemPeriod:
                            device.IsMuted = !device.IsMuted;
                            evt.Handled = true;
                            break;
                        case Key.Right:
                        case Key.OemPlus:
                            device.Volume++;
                            evt.Handled = true;
                            break;
                        case Key.Left:
                        case Key.OemMinus:
                            device.Volume--;
                            evt.Handled = true;
                            break;
                    }
                }
            }
        }
    }
}
