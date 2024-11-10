using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.Helpers;
using EarTrumpet.UI.Themes;
using System;
using System.Windows;
using System.Windows.Media.Animation;
using Windows.Win32;

namespace EarTrumpet.UI.Helpers
{
    public class WindowAnimationLibrary
    {
        const int _animationOffset = 25;

        public static void BeginFlyoutEntranceAnimation(Window window, WindowsTaskbar.State taskbar, Action completed)
        {
            var onCompleted = new EventHandler((s, e) =>
            {
                window.Topmost = true;
                window.Focus();
                PInvoke.SetForegroundWindow(new HWND(window.GetHandle()));
                completed();
            });

            window.Topmost = false;
            window.Activate();
            BringTaskbarToFront();

            if (!Manager.Current.AnimationsEnabled)
            {
                window.Cloak(false);
                onCompleted(null, null);
                return;
            }

            var moveAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(130)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(140)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
                From = 0.8,
                To = 1
            };
            fadeAnimation.Completed += (s, e) => { window.Opacity = 1; };
            Storyboard.SetTarget(fadeAnimation, window);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

            double moveAnimationTo;
            switch (taskbar.Location)
            {
                case WindowsTaskbar.Position.Left:
                    moveAnimationTo = window.Left;
                    window.Left -= _animationOffset;
                    break;
                case WindowsTaskbar.Position.Right:
                    moveAnimationTo = window.Left;
                    window.Left += _animationOffset;
                    break;
                case WindowsTaskbar.Position.Top:
                    moveAnimationTo = window.Top;
                    window.Top -= _animationOffset;
                    break;
                case WindowsTaskbar.Position.Bottom:
                default:
                    moveAnimationTo = window.Top;
                    window.Top += _animationOffset;
                    break;
            }
            moveAnimation.To = moveAnimationTo;

            if (taskbar.Location == WindowsTaskbar.Position.Left || taskbar.Location == WindowsTaskbar.Position.Right)
            {
                Storyboard.SetTarget(moveAnimation, window);
                Storyboard.SetTargetProperty(moveAnimation, new PropertyPath(Window.LeftProperty));
                moveAnimation.From = window.Left;
            }
            else
            {
                Storyboard.SetTarget(moveAnimation, window);
                Storyboard.SetTargetProperty(moveAnimation, new PropertyPath(Window.TopProperty));
                moveAnimation.From = window.Top;
            }

            if (SystemSettings.IsTransparencyEnabled)
            {
                window.Opacity = 0.8;
            }

            window.Cloak(false);

            var storyboard = new Storyboard();
            storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Children.Add(moveAnimation);

            if (SystemSettings.IsTransparencyEnabled)
            {
                storyboard.Children.Add(fadeAnimation);
            }

            storyboard.Completed += (s, e) =>
            {
                if (taskbar.IsHorizontal)
                {
                    window.Top = moveAnimationTo;
                }
                else
                {
                    window.Left = moveAnimationTo;
                }
            };
            storyboard.Completed += onCompleted;

            storyboard.Begin(window);
        }

        public static void BeginFlyoutExitanimation(Window window, Action completed)
        {
            var onCompleted = new EventHandler((s, e) =>
            {
                window.Cloak();
                completed();
            });

            window.Topmost = false;

            if (!Manager.Current.AnimationsEnabled)
            {
                onCompleted(null, null);
                return;
            }

            var moveAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
                From = 1,
                To = 0.75,
            };
            fadeAnimation.Completed += (s, e) => { window.Opacity = 0; };

            Storyboard.SetTarget(fadeAnimation, window);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

            var taskbarPosition = WindowsTaskbar.Current.Location;

            switch (taskbarPosition)
            {
                case WindowsTaskbar.Position.Left:
                    moveAnimation.To = window.Left - _animationOffset;
                    break;
                case WindowsTaskbar.Position.Right:
                    moveAnimation.To = window.Left - _animationOffset;
                    break;
                case WindowsTaskbar.Position.Top:
                    moveAnimation.To = window.Top - _animationOffset;
                    break;
                case WindowsTaskbar.Position.Bottom:
                default:
                    moveAnimation.To = window.Top + _animationOffset;
                    break;
            }

            if (taskbarPosition == WindowsTaskbar.Position.Left || taskbarPosition == WindowsTaskbar.Position.Right)
            {
                Storyboard.SetTarget(moveAnimation, window);
                Storyboard.SetTargetProperty(moveAnimation, new PropertyPath(Window.LeftProperty));
                moveAnimation.From = window.Left;
            }
            else
            {
                Storyboard.SetTarget(moveAnimation, window);
                Storyboard.SetTargetProperty(moveAnimation, new PropertyPath(Window.TopProperty));
                moveAnimation.From = window.Top;
            }

            var storyboard = new Storyboard();
            storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Children.Add(moveAnimation);

            if (SystemSettings.IsTransparencyEnabled)
            {
                storyboard.Children.Add(fadeAnimation);
            }

            storyboard.Completed += onCompleted;
            storyboard.Begin(window);
        }

        public static void BeginWindowExitAnimation(Window window, Action completed)
        {
            var onCompleted = new EventHandler((s, e) =>
            {
                window.Cloak();
                completed();
            });

            if (!Manager.Current.AnimationsEnabled || !SystemSettings.IsTransparencyEnabled)
            {
                window.Dispatcher.BeginInvoke((Action)(() =>
                {
                    onCompleted(null, null);
                }));
            }
            else
            {
                var fadeAnimation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                    From = 1,
                    To = 0.75,
                };
                fadeAnimation.Completed += (s, e) => { window.Opacity = 0; };

                Storyboard.SetTarget(fadeAnimation, window);
                Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

                var storyboard = new Storyboard();
                storyboard.FillBehavior = FillBehavior.Stop;
                storyboard.Children.Add(fadeAnimation);
                storyboard.Completed += onCompleted;
                storyboard.Begin(window);
            }
        }

        public static void BeginWindowEntranceAnimation(Window window, Action completed, double fromOpacity = 0.5)
        {
            var onCompleted = new EventHandler((s, e) =>
            {
                completed();
            });

            if (!Manager.Current.AnimationsEnabled)
            {
                window.Cloak(false);
                onCompleted(null, null);
                return;
            }

            var fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(250)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut },
                From = fromOpacity,
                To = 1,
            };
            fadeAnimation.Completed += (s, e) => { window.Opacity = 1; };

            Storyboard.SetTarget(fadeAnimation, window);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));

            var storyboard = new Storyboard();
            storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Children.Add(fadeAnimation);
            storyboard.Completed += onCompleted;

            window.Cloak(false);

            storyboard.Begin(window);
        }

        public static void BringTaskbarToFront()
        {
            PInvoke.SetForegroundWindow(new HWND(WindowsTaskbar.GetHwnd()));
        }
    }
}
