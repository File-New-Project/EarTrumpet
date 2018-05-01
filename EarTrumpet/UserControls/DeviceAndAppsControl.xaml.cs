using EarTrumpet.Extensions;
using EarTrumpet.Services;
using EarTrumpet.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EarTrumpet.UserControls
{
    public partial class DeviceAndAppsControl : UserControl
    {
        const string s_DragDropDataFormat = "EarTrumpet.AudioSession";

        public DeviceViewModel Device { get { return (DeviceViewModel)GetValue(DeviceProperty); } set { SetValue(DeviceProperty, value); } }
        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(DeviceViewModel), typeof(DeviceAndAppsControl), new PropertyMetadata(null));

        public DeviceAndAppsControl()
        {
            InitializeComponent();
            GridRoot.DataContext = this;

            UpdateTheme();
            ThemeService.ThemeChanged += UpdateTheme;
        }

        ~DeviceAndAppsControl()
        {
            ThemeService.ThemeChanged -= UpdateTheme;
        }

        void UpdateTheme()
        {
            ThemeService.UpdateThemeResources(Resources);
        }

        Point _mouseDownPoint;
        private void ListView_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _mouseDownPoint = e.GetPosition(null);
        }

        private void ListView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = _mouseDownPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
               Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var listView = sender as ListView;
                var listViewItem = ((DependencyObject)e.OriginalSource).FindAnchestor<ListViewItem>();

                if (listViewItem != null)
                {
                    var ptInListViewItem = e.GetPosition(listViewItem);

                    // TODO: hack until we figure out D&D and slider behavior.
                    if (ptInListViewItem.X > listViewItem.ActualWidth - 50)
                    {
                        var vm = (AudioSessionViewModel)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

                        DataObject dragData = new DataObject(s_DragDropDataFormat, vm);
                        DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move | DragDropEffects.Link);
                    }
                }
            }
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(s_DragDropDataFormat))
            {
                var vm = e.Data.GetData(s_DragDropDataFormat) as AudioSessionViewModel;
                Device.TakeExternalSession(vm);
            }
        }

        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(s_DragDropDataFormat) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }
        }
    }
}
