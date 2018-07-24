using System;

namespace EarTrumpet.UI.ViewModels
{
    public interface IWindowHostedViewModel
    {
        event Action RequestClose;
    }
}
