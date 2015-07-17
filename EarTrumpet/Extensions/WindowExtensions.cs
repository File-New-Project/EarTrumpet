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
            DoubleAnimation hideAnimation = new DoubleAnimation();
            hideAnimation.Duration = new Duration(slidetime);
            var taskbarPosition = TaskbarService.TaskbarPosition;
            switch (taskbarPosition)
            {
                case TaskbarPosition.Top: hideAnimation.From = window.Top; break;
                case TaskbarPosition.Bottom: hideAnimation.From = window.Top; break;
                case TaskbarPosition.Left: hideAnimation.From = window.Left; break;
                case TaskbarPosition.Right: hideAnimation.From = window.Left; break;
                default: hideAnimation.From = window.Top; break;
            }
            hideAnimation.To = (taskbarPosition == TaskbarPosition.Top || taskbarPosition == TaskbarPosition.Left) ? hideAnimation.From - 10 : hideAnimation.From + 10;
            hideAnimation.FillBehavior = FillBehavior.Stop;
            var easing = new QuinticEase(); 
            easing.EasingMode = EasingMode.EaseIn;
            hideAnimation.EasingFunction = easing;
            hideAnimation.Completed += (s, e) =>
            {
                window.Visibility = Visibility.Hidden;
            };

            switch (taskbarPosition)
            {
                case TaskbarPosition.Top:
                case TaskbarPosition.Bottom:
                    window.ApplyAnimationClock(Window.TopProperty, hideAnimation.CreateClock()); 
                    break;
                case TaskbarPosition.Left: 
                case TaskbarPosition.Right:
                    window.ApplyAnimationClock(Window.LeftProperty, hideAnimation.CreateClock());  
                    break;
                default:
                    window.ApplyAnimationClock(Window.TopProperty, hideAnimation.CreateClock()); 
                    break;
            }
        }

        public static void ShowwithAnimation(this Window window)
        {            
            window.Visibility = Visibility.Visible;
            window.Topmost = false;
            window.Activate();
            TimeSpan slidetime = TimeSpan.FromSeconds(0.3);
            DoubleAnimation showAnimation = new DoubleAnimation();
            showAnimation.Duration = new Duration(slidetime);
            var taskbarPosition = TaskbarService.TaskbarPosition;
            switch (taskbarPosition)
            {
                case TaskbarPosition.Top: showAnimation.To = window.Top; break;
                case TaskbarPosition.Bottom: showAnimation.To = window.Top; break;
                case TaskbarPosition.Left: showAnimation.To = window.Left; break; // + window.Width
                case TaskbarPosition.Right: showAnimation.To = window.Left; break;
                default: showAnimation.To = window.Top; break;
            }           
            showAnimation.From = (taskbarPosition == TaskbarPosition.Top || taskbarPosition == TaskbarPosition.Left) ? showAnimation.To - 25 : showAnimation.To + 25;            
            showAnimation.FillBehavior = FillBehavior.Stop;
            showAnimation.Completed += (s, e) =>
            {
                window.Topmost = true;
                window.Focus();                
            };
            var easing = new QuinticEase();
            easing.EasingMode = EasingMode.EaseOut;
            showAnimation.EasingFunction = easing;
            switch (taskbarPosition)
            {
                case TaskbarPosition.Top:
                case TaskbarPosition.Bottom:
                    window.ApplyAnimationClock(Window.TopProperty, showAnimation.CreateClock());
                    break;
                case TaskbarPosition.Left: 
                case TaskbarPosition.Right:
                    window.ApplyAnimationClock(Window.LeftProperty, showAnimation.CreateClock());
                    break;
                default:
                    window.ApplyAnimationClock(Window.TopProperty, showAnimation.CreateClock());
                    break;
            }
        }
    }
}
