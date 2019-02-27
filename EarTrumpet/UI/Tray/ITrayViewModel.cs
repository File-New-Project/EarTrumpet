using EarTrumpet.UI.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Input;

namespace EarTrumpet.UI.Tray
{
    public interface ITrayViewModel : INotifyPropertyChanged
    {
        ICommand LeftClick { get; set; }
        ICommand MiddleClick { get; }
        IEnumerable<ContextMenuItem> MenuItems { get; }
        ICommand OpenMixer { get; set; }
        string ToolTip { get; }
        Icon TrayIcon { get; }
        void Refresh();
    }
}