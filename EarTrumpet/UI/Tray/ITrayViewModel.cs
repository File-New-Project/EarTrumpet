using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Input;

namespace EarTrumpet.UI.Tray
{
    public interface ITrayViewModel : INotifyPropertyChanged
    {
        Func<Guid> GetIdentity { get; }
        Action ResetIdentity { get; }
        ICommand LeftClick { get; set; }
        ICommand MiddleClick { get; }
        IEnumerable<ContextMenuItem> MenuItems { get; }
        ICommand OpenMixer { get; set; }
        string ToolTip { get; }
        Icon TrayIcon { get; }
        void Refresh();
    }
}