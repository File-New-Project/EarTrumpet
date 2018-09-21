using System;
using System.Windows.Input;

namespace EarTrumpet.UI.ViewModels
{
    public interface IWindowHostedViewModel
    {
        event Action Close;
        event Action<object> HostDialog;
        void OnClosing();
        void OnPreviewKeyDown(KeyEventArgs e);
    }
}
