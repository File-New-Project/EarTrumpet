using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace EarTrumpet.UI.Tray
{
    public interface ITrayViewModel : INotifyPropertyChanged
    {
        RelayCommand LeftClick { get; }
        IEnumerable<ContextMenuItem> MenuItems { get; }
        RelayCommand MiddleClick { get; }
        string ToolTip { get; }
        Icon TrayIcon { get; }
        void Refresh();
    }
}