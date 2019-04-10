﻿using EarTrumpet.UI.ViewModels;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace EarTrumpet.UI.Views
{
    public partial class ModalDialogChrome : UserControl
    {
        public ModalDialogChrome()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var dt = (ToolbarItemViewModel)btn.DataContext;

            if (dt.Menu != null)
            {
                btn.ContextMenu.Opened += (_, __) =>
                {
                    ((Popup)btn.ContextMenu.Parent).PopupAnimation = PopupAnimation.None;
                };

                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void Button_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var btn = (Button)sender;
            var dt = (ToolbarItemViewModel)btn.DataContext;

            if (dt.Menu == null)
            {
                btn.ContextMenu = null;
            }
        }
    }
}
