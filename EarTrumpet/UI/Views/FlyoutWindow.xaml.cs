using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Threading;

namespace EarTrumpet.UI.Views
{
    public partial class FlyoutWindow
    {
        private readonly IFlyoutViewModel _viewModel;

        public FlyoutWindow(IFlyoutViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;

            InitializeComponent();

            _viewModel.StateChanged += OnStateChanged;
            _viewModel.WindowSizeInvalidated += (_, __) => PositionWindowRelativeToTaskbar();
            SourceInitialized += (_, __) => this.Cloak();
            Themes.Manager.Current.ThemeChanged += EnableOrDisableAcrylic;
        }

        public void Initialize()
        {
            Show();
            Hide();
            // Prevent showing up in Alt+Tab.
            this.ApplyExtendedWindowStyle(User32.WS_EX_TOOLWINDOW);

            _viewModel.ChangeState(FlyoutViewState.Hidden);
        }

        private void OnStateChanged(object sender, object e)
        {
            switch (_viewModel.State)
            {
                case FlyoutViewState.Opening:
                    Show();
                    EnableOrDisableAcrylic();
                    PositionWindowRelativeToTaskbar();

                    // Focus the first device if available.
                    DevicesList.FindVisualChild<DeviceView>()?.FocusAndRemoveFocusVisual();

                    // Prevent showing stale adnorners.
                    this.WaitForKeyboardVisuals(() =>
                    {
                        WindowAnimationLibrary.BeginFlyoutEntranceAnimation(this, () =>
                        {
                            _viewModel.ChangeState(FlyoutViewState.Open);
                        });
                    });
                    break;

                case FlyoutViewState.Closing_Stage1:
                    DevicesList.FindVisualChild<DeviceView>()?.FocusAndRemoveFocusVisual();

                    if (_viewModel.IsExpandingOrCollapsing)
                    {
                        WindowAnimationLibrary.BeginFlyoutExitanimation(this, () =>
                        {
                            this.Cloak();
                            EnableOrDisableAcrylic();

                            // Go directly to ViewState.Hidden to avoid the stage 2 hide delay (debounce for tray clicks),
                            // we want to show again immediately.
                            _viewModel.ChangeState(FlyoutViewState.Hidden);
                        });
                    }
                    else
                    {
                        // No animation for normal exit.
                        this.Cloak();
                        EnableOrDisableAcrylic();

                        // Prevent de-queueing partially on show and showing stale adnorners.
                        this.WaitForKeyboardVisuals(() =>
                        {
                            Hide();
                            _viewModel.ChangeState(FlyoutViewState.Closing_Stage2);
                        });
                    }
                    break;
            }
        }

        private void PositionWindowRelativeToTaskbar()
        {
            var taskbarState = WindowsTaskbar.Current;

            if (taskbarState.ContainingScreen == null)
            {
                // We're not ready to lay out. (e.g. RDP transition)
                return;
            }

            UpdateLayout();
            LayoutRoot.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            double newHeight = LayoutRoot.DesiredSize.Height;

            var scaledWorkAreaHeight = taskbarState.ContainingScreen.WorkingArea.Height / this.DpiHeightFactor();
            if (taskbarState.IsAutoHideEnabled && (taskbarState.Location == WindowsTaskbar.Position.Top || taskbarState.Location == WindowsTaskbar.Position.Bottom))
            {
                scaledWorkAreaHeight -= taskbarState.Size.Bottom - taskbarState.Size.Top;
            }

            if (newHeight > scaledWorkAreaHeight)
            {
                newHeight = scaledWorkAreaHeight;
            }

            bool isRTL = SystemSettings.IsRTL;
            double newTop = 0;
            double newLeft = 0;

            switch (taskbarState.Location)
            {
                case WindowsTaskbar.Position.Left:
                    newLeft = (taskbarState.Size.Right / this.DpiWidthFactor());
                    newTop = (taskbarState.Size.Bottom / this.DpiHeightFactor()) - newHeight;
                    break;
                case WindowsTaskbar.Position.Right:
                    newLeft = (taskbarState.Size.Left / this.DpiWidthFactor()) - Width;
                    newTop = (taskbarState.Size.Bottom / this.DpiHeightFactor()) - newHeight;
                    break;
                case WindowsTaskbar.Position.Top:
                    newLeft = isRTL ? (taskbarState.Size.Left / this.DpiWidthFactor()) :
                        (taskbarState.Size.Right / this.DpiWidthFactor()) - Width;
                    newTop = (taskbarState.Size.Bottom / this.DpiHeightFactor());
                    break;
                case WindowsTaskbar.Position.Bottom:
                    newLeft = isRTL ? (taskbarState.Size.Left / this.DpiWidthFactor()) :
                        (taskbarState.Size.Right / this.DpiWidthFactor()) - Width;
                    newTop = (taskbarState.Size.Top / this.DpiHeightFactor()) - newHeight;
                    break;
            }

            this.Move(newTop * this.DpiHeightFactor(), newLeft * this.DpiWidthFactor(), newHeight * this.DpiHeightFactor(), Width * this.DpiWidthFactor());
        }

        private void EnableOrDisableAcrylic()
        {
            if (_viewModel.State == FlyoutViewState.Opening || _viewModel.State == FlyoutViewState.Open)
            {
                AccentPolicyLibrary.EnableAcrylic(this, Themes.Manager.Current.ResolveRef(this, "AcrylicColor_Flyout"), GetAccentFlags());
            }
            else
            {
                AccentPolicyLibrary.DisableAcrylic(this);
            }
        }

        private User32.AccentFlags GetAccentFlags()
        {
            switch (WindowsTaskbar.Current.Location)
            {
                case WindowsTaskbar.Position.Left:
                    return User32.AccentFlags.DrawRightBorder | User32.AccentFlags.DrawTopBorder;
                case WindowsTaskbar.Position.Right:
                    return User32.AccentFlags.DrawLeftBorder | User32.AccentFlags.DrawTopBorder;
                case WindowsTaskbar.Position.Top:
                    return User32.AccentFlags.DrawLeftBorder | User32.AccentFlags.DrawBottomBorder;
                case WindowsTaskbar.Position.Bottom:
                    return User32.AccentFlags.DrawTopBorder | User32.AccentFlags.DrawLeftBorder;
            }
            return User32.AccentFlags.None;
        }
    }
}
