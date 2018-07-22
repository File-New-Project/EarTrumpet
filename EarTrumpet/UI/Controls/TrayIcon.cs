using EarTrumpet.Properties;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace EarTrumpet.UI.Controls
{
    class TrayIcon
    {
        private readonly System.Windows.Forms.NotifyIcon _trayIcon;
        private readonly TrayViewModel _trayViewModel;

        public TrayIcon(TrayViewModel trayViewModel)
        {
            _trayViewModel = trayViewModel;
            _trayViewModel.PropertyChanged += TrayViewModel_PropertyChanged;

            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.MouseClick += TrayIcon_MouseClick;
            _trayIcon.Icon = _trayViewModel.TrayIcon;
            _trayIcon.Text = _trayViewModel.ToolTip;

            App.Current.Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }

        private ContextMenu BuildContextMenu()
        {
            var cm = ThemedContextMenu.CreateThemedContextMenu();

            // Add devices
            var audioDevices = _trayViewModel.AllDevices.OrderBy(x => x.DisplayName);
            if (!audioDevices.Any())
            {
                ThemedContextMenu.AddItem(cm, new MenuItem
                {
                    Header = Resources.ContextMenuNoDevices,
                    IsEnabled = false,
                });
            }
            else
            {
                foreach (var device in audioDevices)
                {
                    ThemedContextMenu.AddItem(cm, new MenuItem
                    {
                        Header = device.DisplayName,
                        IsChecked = device.Id == _trayViewModel.DefaultDeviceId,
                        Command = new RelayCommand(() => _trayViewModel.ChangeDeviceCommand.Execute(device)),
                    });
                }
            }

            var staticItems = _trayViewModel.StaticCommands.ToList();
            var addonItems = _trayViewModel.GetAddons();

            foreach (var contextMenuExtension in Extensibility.Hosting.AddonHostService.Instance.ContextMenuItems)
            {
                var extensionSubItems = new List<Tuple<string, object>>();
                foreach (var ext in contextMenuExtension.Items)
                {
                    extensionSubItems.Add(new Tuple<string, object>(ext.Item1, new RelayCommand(ext.Item2)));
                }

                addonItems.Add(new Tuple<string, object>(contextMenuExtension.DisplayName, extensionSubItems));
            }

            if (addonItems.Count > 0)
            {
                var addonSection = new List<Tuple<string, object>>();
                addonSection.Add(new Tuple<string, object>("Addons", addonItems)); // TODO: localize
                staticItems.Insert(staticItems.Count - 1, addonSection);
            }

            AddItems(cm, staticItems);

            return cm;
        }

        private void AddItems(ItemsControl menu, List<IEnumerable<Tuple<string, object>>> items)
        {
            foreach (var bucket in items)
            {
                if (items.Count > 1)
                {
                    menu.Items.Add(new Separator { Style = (Style)Application.Current.FindResource("MenuItemSeparatorDarkOnly") });
                }

                foreach (var item in bucket)
                {
                    if (item.Item2 is RelayCommand)
                    {
                        ThemedContextMenu.AddItem(menu, item.Item1, (RelayCommand)item.Item2);
                    }
                    else
                    {
                        var subItems = (IEnumerable<Tuple<string, object>>)item.Item2;
                        var subItemWrapper = new List<IEnumerable<Tuple<string, object>>>();
                        subItemWrapper.Add(subItems);

                        var pivotNode = ThemedContextMenu.AddItem(menu, item.Item1, null);
                        AddItems(pivotNode, subItemWrapper);
                    }
                }
            }
        }

        public void Show()
        {
            Trace.WriteLine("TrayIcon Show");
            _trayIcon.Visible = true;
            Trace.WriteLine("TrayIcon Shown");
        }

        private void TrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_trayViewModel.TrayIcon))
            {
                _trayIcon.Icon = _trayViewModel.TrayIcon;
            }
            else if (e.PropertyName == nameof(_trayViewModel.ToolTip))
            {
                _trayIcon.Text = _trayViewModel.ToolTip;
            }
        }

        void TrayIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Trace.WriteLine($"TrayIcon TrayIcon_MouseClick {e.Button}");

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _trayViewModel.OpenFlyoutCommand.Execute();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var cm = BuildContextMenu();
                cm.Placement = PlacementMode.Mouse;
                cm.IsOpen = true;
                Trace.WriteLine("TrayIcon TrayIcon_MouseClick Right (ContextMenu now open)");
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                _trayViewModel.ToggleMute();
            }
        }
    }
}
