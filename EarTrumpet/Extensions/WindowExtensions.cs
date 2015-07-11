using EarTrumpet.Services;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace EarTrumpet.Extensions
{
    internal static class WindowExtensions
    {
        public static void HideWithAnimation(this Window window)
        {
            TimeSpan slidetime = TimeSpan.FromSeconds(0.2);
            DoubleAnimation topAnimation = new DoubleAnimation();
            topAnimation.Duration = new Duration(slidetime);
            topAnimation.From = window.Top;
            topAnimation.To = window.Top + 10;
            topAnimation.FillBehavior = FillBehavior.Stop;
            var easing = new QuinticEase(); 
            easing.EasingMode = EasingMode.EaseIn;
            topAnimation.EasingFunction = easing;
            topAnimation.Completed += (s, e) =>
            {
                window.Visibility = Visibility.Hidden;
            };
            window.BeginAnimation(Window.TopProperty, topAnimation); 
        }

        public static void ShowwithAnimation(this Window window)
        {
            window.Visibility = Visibility.Visible;
            window.Topmost = false;            
            TimeSpan slidetime = TimeSpan.FromSeconds(0.3);
            DoubleAnimation bottomAnimation = new DoubleAnimation();
            bottomAnimation.Duration = new Duration(slidetime);
            double top = window.Top;
            bottomAnimation.From = window.Top + 25;
            bottomAnimation.To = window.Top;
            bottomAnimation.FillBehavior = FillBehavior.Stop;
            bottomAnimation.Completed += (s, e) =>
            {
                window.Topmost = true;
                // Set the final position again. This covers a case where frames are dropped.
                // and the window ends up over the taskbar instead.
                window.Top = top;
                window.Activate();
                window.Focus();
            };
            var easing = new QuinticEase();
            easing.EasingMode = EasingMode.EaseOut;
            bottomAnimation.EasingFunction = easing;
            window.BeginAnimation(Window.TopProperty, bottomAnimation);
        }
    }
}
