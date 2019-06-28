using EarTrumpet.UI.Helpers;
using System;

namespace EarTrumpet.UI.ViewModels
{
    public interface IFlyoutViewModel
    {
        FlyoutViewState State { get; }
        bool IsExpandingOrCollapsing { get; }

        event EventHandler<object> StateChanged;
        event EventHandler<object> WindowSizeInvalidated;

        void ChangeState(FlyoutViewState state);
    }
}