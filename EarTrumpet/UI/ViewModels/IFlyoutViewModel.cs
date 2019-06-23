using EarTrumpet.UI.Helpers;
using System;

namespace EarTrumpet.UI.ViewModels
{
    public interface IFlyoutViewModel
    {
        ViewState State { get; }
        bool IsExpandingOrCollapsing { get; }

        event EventHandler<object> StateChanged;
        event EventHandler<object> WindowSizeInvalidated;

        void ChangeState(ViewState state);
    }
}