﻿using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Helpers;
using EarTrumpet.UI.ViewModels;
using System.Windows;
using System.Windows.Media;

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
            SourceInitialized += (_, __) => this.Cloak();
            Themes.Manager.Current.ThemeChanged += () => EnableAcrylicIfApplicable(WindowsTaskbar.Current);
            LayoutRoot.Background = new SolidColorBrush(Themes.Manager.Current.ResolveRef(this, "AcrylicColor_Flyout"));
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
                    PositionWindowRelativeToTaskbar(taskbar);

                    // Focus the first device if available.
                    DevicesList.FindVisualChild<DeviceView>()?.FocusAndRemoveFocusVisual();

                    // Prevent showing stale adnorners.
                    this.WaitForKeyboardVisuals(() =>
                    {
                        LayoutRoot.Background = new SolidColorBrush(Themes.Manager.Current.ResolveRef(this, "AcrylicColor_Flyout"));
                        WindowAnimationLibrary.BeginFlyoutEntranceAnimation(this, taskbar, () =>
                        {
                            _viewModel.ChangeState(FlyoutViewState.Open);
                            // Wait to apply acrylic, or the translate transform illusion will be spoiled.
                            EnableAcrylicIfApplicable(taskbar);
                        });
                    });
                    break;

                case FlyoutViewState.Closing_Stage1:
                    DevicesList.FindVisualChild<DeviceView>()?.FocusAndRemoveFocusVisual();

                    if (_viewModel.IsExpandingOrCollapsing)
                    {
                        AccentPolicyLibrary.DisableAcrylic(this);
                        WindowAnimationLibrary.BeginFlyoutExitanimation(this, () =>
                        {
                            this.Cloak();

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

            // WorkArea accounts for normal taskbar and docked windows.
            var maxHeight = taskbar.ContainingScreen.WorkingArea.Height;
            if (taskbar.IsAutoHideEnabled && (taskbar.Location == WindowsTaskbar.Position.Top || taskbar.Location == WindowsTaskbar.Position.Bottom))
            {
                // AutoHide Taskbar won't carve space out for itself, so manually account for the Top or Bottom Taskbar height.
                // Note: Ideally we would open our flyout and 'hold open' the taskbar, but it's not known how to command the Taskbar
                // to stay open for the duration of our window being active.
                maxHeight -= taskbar.Size.Bottom - taskbar.Size.Top;
            }

            double newWidth = Width * this.DpiX();
            double newHeight = LayoutRoot.DesiredSize.Height * this.DpiY();
            if (newHeight > maxHeight)
            {
                newHeight = maxHeight;
            }

            switch (taskbar.Location)
            {
                case WindowsTaskbar.Position.Left:
                    this.SetWindowPos(taskbar.Size.Bottom - newHeight,
                              taskbar.Size.Right,
                              newHeight,
                              newWidth);
                    break;
                case WindowsTaskbar.Position.Right:
                    this.SetWindowPos(taskbar.Size.Bottom - newHeight,
                              taskbar.Size.Left - newWidth,
                              newHeight,
                              newWidth);
                    break;
                case WindowsTaskbar.Position.Top:
                    this.SetWindowPos(taskbar.Size.Bottom,
                              FlowDirection == FlowDirection.RightToLeft ? taskbar.Size.Left : taskbar.Size.Right - newWidth,
                              newHeight,
                              newWidth);
                    break;
                case WindowsTaskbar.Position.Bottom:
                    this.SetWindowPos(taskbar.Size.Top - newHeight,
                              FlowDirection == FlowDirection.RightToLeft ? taskbar.Size.Left : taskbar.Size.Right - newWidth,
                              newHeight,
                              newWidth);
                    break;
            }
        }

        private void EnableAcrylicIfApplicable(WindowsTaskbar.State taskbar)
        {
            // Note: Enable when in Opening as well as Open in case we get a theme change during a show cycle.
            if (_viewModel.State == FlyoutViewState.Opening || _viewModel.State == FlyoutViewState.Open)
            {
                AccentPolicyLibrary.EnableAcrylic(this, Colors.Transparent, GetAccentFlags(taskbar));
            }
            else
            {
                // Disable to avoid visual issues like showing a pane of acrylic while we're Hidden+cloaked.
                AccentPolicyLibrary.DisableAcrylic(this);
            }
        }

        private User32.AccentFlags GetAccentFlags(WindowsTaskbar.State taskbar)
        {
            switch (taskbar.Location)
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
