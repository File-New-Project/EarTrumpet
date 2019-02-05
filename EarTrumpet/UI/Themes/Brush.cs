using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EarTrumpet.UI.Themes
{
    public static class Brush
    {
        public static string GetForeground(DependencyObject obj) => (string)obj.GetValue(ForegroundProperty);
        public static void SetForeground(DependencyObject obj, string value) => obj.SetValue(ForegroundProperty, value);
        public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.RegisterAttached("Foreground", typeof(string), typeof(Brush), new PropertyMetadata("", ForegroundChanged));

        private static void ForegroundChanged(FrameworkElement d, string value)
        {
            try
            {
                var controller = (Controller)d.FindResource("ThemeController");
                if (d is TextBlock)
                {
                    var txt = (TextBlock)d;
                    txt.Foreground = controller.ResolveBrush(value);
                    controller.PropertyChanged += (_, __) => txt.Foreground = controller.ResolveBrush(value);
                }
            }
            catch (ResourceReferenceKeyNotFoundException)
            {
                var target = d as FrameworkElement;
                while (target.Parent != null)
                {
                    target = target.Parent as FrameworkElement;
                }
                target.Loaded += (_, __) => ForegroundChanged(d, value);
            }
        }
        private static void ForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ForegroundChanged((FrameworkElement)d, (string)e.NewValue);
        }

        public static string GetBackground(DependencyObject obj) => (string)obj.GetValue(BackgroundProperty);
        public static void SetBackground(DependencyObject obj, string value) => obj.SetValue(BackgroundProperty, value);
        public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.RegisterAttached("Background", typeof(string), typeof(Brush), new PropertyMetadata("", BackgroundChanged));

        private static void BackgroundChanged(FrameworkElement targetObject, string value)
        {
            try
            {
                var controller = (Controller)targetObject.FindResource("ThemeController");
                if (targetObject is TextBlock)
                {
                    var txt = (TextBlock)targetObject;
                    txt.Background = controller.ResolveBrush(value);
                    controller.PropertyChanged += (_, __) => txt.Background = controller.ResolveBrush(value);
                }
                else if (targetObject is Panel)
                {
                    var panel = (Panel)targetObject;
                    panel.Background = controller.ResolveBrush(value);
                    controller.PropertyChanged += (_, __) => panel.Background = controller.ResolveBrush(value);
                }
            }
            catch (ResourceReferenceKeyNotFoundException)
            {
                var target = targetObject as FrameworkElement;
                while (target.Parent != null)
                {
                    target = target.Parent as FrameworkElement;
                }
                target.Loaded += (_, __) => BackgroundChanged(targetObject, value);
            }
        }
        private static void BackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BackgroundChanged((FrameworkElement)d, (string)e.NewValue);
        }

        public static string GetBorderBrush(DependencyObject obj) => (string)obj.GetValue(BorderBrushProperty);
        public static void SetBorderBrush(DependencyObject obj, string value) => obj.SetValue(BorderBrushProperty, value);
        public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.RegisterAttached("BorderBrush", typeof(string), typeof(Brush), new PropertyMetadata("", BorderBrushChanged));

        private static void BorderBrushChanged(FrameworkElement d, string value)
        {
            try
            {
                var controller = (Controller)d.FindResource("ThemeController");
                if (d is Border)
                {
                    var border = (Border)d;
                    border.BorderBrush = controller.ResolveBrush(value);
                    controller.PropertyChanged += (_, __) => border.BorderBrush = controller.ResolveBrush(value);
                }
            }
            catch (ResourceReferenceKeyNotFoundException)
            {
                var target = d as FrameworkElement;
                while (target.Parent != null)
                {
                    target = target.Parent as FrameworkElement;
                }
                target.Loaded += (_, __) => BorderBrushChanged(d, value);
            }
        }
        private static void BorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BorderBrushChanged((FrameworkElement)d, (string)e.NewValue);
        }
    }
}
