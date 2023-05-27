using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System;
using System.Windows;

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
            _viewModel.WindowSizeInvalidated += OnWindowsSizeInvalidated;
            SourceInitialized += (_, __) =>
            {
                this.Cloak();
                this.EnableRoundedCornersIfApplicable();
            };
            Themes.Manager.Current.ThemeChanged += () => EnableAcrylicIfApplicable(WindowsTaskbar.Current);
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
                    var taskbar = WindowsTaskbar.Current;

                    Show();
                    EnableAcrylicIfApplicable(taskbar);
                    PositionWindowRelativeToTaskbar(taskbar);

                    // Focus the first device if available.
                    DevicesList.FindVisualChild<DeviceView>()?.FocusAndRemoveFocusVisual();

                    // Prevent showing stale adnorners.
                    this.WaitForKeyboardVisuals(() =>
                    {
                        WindowAnimationLibrary.BeginFlyoutEntranceAnimation(this, taskbar, () =>
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
                            AccentPolicyLibrary.DisableAcrylic(this);
                
                            // Go directly to ViewState.Hidden to avoid the stage 2 hide delay (debounce for tray clicks),
                            // we want to show again immediately.
                            _viewModel.ChangeState(FlyoutViewState.Hidden);
                        });
                    }
                    else
                    {
                        // No animation for normal exit.
                        this.Cloak();
                        AccentPolicyLibrary.DisableAcrylic(this);
                
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

        private void OnWindowsSizeInvalidated(object sender, object e)
        {
            // Avoid doing extra work in the background, only update the window when we're actually visible.
            switch (_viewModel.State)
            {
                case FlyoutViewState.Open:
                case FlyoutViewState.Opening:
                    PositionWindowRelativeToTaskbar(WindowsTaskbar.Current);
                    break;
            }
        }

        private void PositionWindowRelativeToTaskbar(WindowsTaskbar.State taskbar)
        {
            // We're not ready if we don't have a taskbar and monitor. (e.g. RDP transition)
            if (taskbar.ContainingScreen == null)
            {
                return;
            }

            // Force layout so we can be sure lists have created/removed containers.
            UpdateLayout();
            LayoutRoot.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            // Working area accounts for normal taskbar and docked windows.
            var adjustedWorkingAreaRight = taskbar.ContainingScreen.WorkingArea.Right;
            var adjustedWorkingAreaLeft = taskbar.ContainingScreen.WorkingArea.Left;
            var adjustedWorkingAreaTop = taskbar.ContainingScreen.WorkingArea.Top;
            var adjustedWorkingAreaBottom = taskbar.ContainingScreen.WorkingArea.Bottom;

            // Taskbar won't carve space out for itself if it's configured to auto-hide, so manually
            // adjust the working area to compensate. This is only done if the working area edge
            // reaches into Taskbar space. This accounts for 0..n docked windows that may not
            // push the working area out far enough.

            if (taskbar.IsAutoHideEnabled)
            {
                switch (taskbar.Location)
                {
                    case WindowsTaskbar.Position.Left:
                        if (taskbar.ContainingScreen.WorkingArea.Left < taskbar.Size.Right)
                        {
                            adjustedWorkingAreaLeft = taskbar.Size.Right;
                        }
                        break;
                    case WindowsTaskbar.Position.Right:
                        if (taskbar.ContainingScreen.WorkingArea.Right > taskbar.Size.Left)
                        {
                            adjustedWorkingAreaRight = taskbar.Size.Left;
                        }
                        break;
                    case WindowsTaskbar.Position.Top:
                        if (taskbar.ContainingScreen.WorkingArea.Top < taskbar.Size.Bottom)
                        {
                            adjustedWorkingAreaTop = taskbar.Size.Bottom;
                        }
                        break;
                    case WindowsTaskbar.Position.Bottom:
                        if (taskbar.ContainingScreen.WorkingArea.Bottom > taskbar.Size.Top)
                        {
                            adjustedWorkingAreaBottom = taskbar.Size.Top;
                        }
                        break;
                }
            }

            double flyoutWidth = Width * this.DpiX();
            double flyoutHeight = (LayoutRoot.DesiredSize.Height) * this.DpiY();

            double yOffset = 0;
            double xOffset = 0;
            if(Environment.OSVersion.IsAtLeast(OSVersions.Windows11))
            {
                xOffset += 12 * this.DpiX();
                yOffset += 12 * this.DpiY();
            }

            var workingAreaHeight = Math.Abs(adjustedWorkingAreaTop - adjustedWorkingAreaBottom) - (yOffset * 2);
            if (flyoutHeight > workingAreaHeight)
            {
                flyoutHeight = workingAreaHeight;
            }

            double top = 0;
            double left = 0;
            switch (taskbar.Location)
            {
                case WindowsTaskbar.Position.Left:
                    top = adjustedWorkingAreaBottom - flyoutHeight;
                    left = adjustedWorkingAreaLeft;
                    break;
                case WindowsTaskbar.Position.Right:
                    top = adjustedWorkingAreaBottom - flyoutHeight;
                    left = adjustedWorkingAreaRight - flyoutWidth;
                    break;
                case WindowsTaskbar.Position.Top:
                    top = adjustedWorkingAreaTop + xOffset;
                    left = FlowDirection == FlowDirection.LeftToRight ? adjustedWorkingAreaRight - flyoutWidth - xOffset : adjustedWorkingAreaLeft + xOffset;
                    break;
                case WindowsTaskbar.Position.Bottom:
                    top = adjustedWorkingAreaBottom - flyoutHeight - yOffset;
                    left = FlowDirection == FlowDirection.LeftToRight ? adjustedWorkingAreaRight - flyoutWidth - xOffset : adjustedWorkingAreaLeft + xOffset;
                    break;
            }
            this.SetWindowPos(top, left, flyoutHeight, flyoutWidth);
            _viewModel.UpdateWindowPos(top, left, flyoutHeight, flyoutWidth);
        }

        private void EnableAcrylicIfApplicable(WindowsTaskbar.State taskbar)
        {
            // Note: Enable when in Opening as well as Open in case we get a theme change during a show cycle.
            if (_viewModel.State == FlyoutViewState.Opening || _viewModel.State == FlyoutViewState.Open)
            {
                AccentPolicyLibrary.EnableAcrylic(this, Themes.Manager.ResolveRef(this, "AcrylicColor_Flyout"), GetAccentFlags(taskbar));
            }
            else
            {
                // Disable to avoid visual issues like showing a pane of acrylic while we're Hidden+cloaked.
                AccentPolicyLibrary.DisableAcrylic(this);
            }
        }

        private static User32.AccentFlags GetAccentFlags(WindowsTaskbar.State taskbar)
        {
            if (Environment.OSVersion.IsAtLeast(OSVersions.Windows11))
            {
                return User32.AccentFlags.DrawAllBorders;
            }

            return taskbar.Location switch
            {
                WindowsTaskbar.Position.Left => User32.AccentFlags.DrawRightBorder | User32.AccentFlags.DrawTopBorder,
                WindowsTaskbar.Position.Right => User32.AccentFlags.DrawLeftBorder | User32.AccentFlags.DrawTopBorder,
                WindowsTaskbar.Position.Top => User32.AccentFlags.DrawLeftBorder | User32.AccentFlags.DrawBottomBorder,
                WindowsTaskbar.Position.Bottom => User32.AccentFlags.DrawTopBorder | User32.AccentFlags.DrawLeftBorder,
                _ => User32.AccentFlags.None,
            };
        }
    }
}
