using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using EarTrumpet.UI.Helpers;

namespace EarTrumpet.UI.ViewModels
{
    public interface ITrayViewModel : INotifyPropertyChanged
    {
        RelayCommand LeftClick { get; }
        IEnumerable<ContextMenuItem> MenuItems { get; }
        RelayCommand MiddleClick { get; }
        RelayCommand RightClick { get; }
        string ToolTip { get; }
        Icon TrayIcon { get; }
        void DpiChanged();

        event Action ContextMenuRequested;
    }
}