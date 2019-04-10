using System.Windows;

namespace EarTrumpet.UI.ViewModels
{
    public interface IPopupHostViewModel
    {
        void OpenPopup(object vm, FrameworkElement container);
    }
}