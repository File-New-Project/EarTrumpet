using EarTrumpet.DataModel;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace EarTrumpet.UI.Behaviors
{
    public class FrameworkElementEx
    {
        public enum FlowDirectionKind
        {
            Auto,
            Unset
        }

        // FlowDirection: Set RTL in XAML.
        public static FlowDirectionKind GetFlowDirection(DependencyObject obj) => (FlowDirectionKind)obj.GetValue(FlowDirectionProperty);
        public static void SetFlowDirection(DependencyObject obj, FlowDirectionKind value) => obj.SetValue(FlowDirectionProperty, value);
        public static readonly DependencyProperty FlowDirectionProperty =DependencyProperty.RegisterAttached(
            "FlowDirection", typeof(FlowDirectionKind), typeof(FrameworkElementEx), new PropertyMetadata(FlowDirectionKind.Unset, OnFlowDirectionChanged));

        private static void OnFlowDirectionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert((FlowDirectionKind)e.NewValue == FlowDirectionKind.Auto);
            ((FrameworkElement)dependencyObject).FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        // DisplaySettingsChanged: Get UI notification of DisplaySettingsChanged.
        public static ICommand GetDisplaySettingsChanged(DependencyObject obj) => (ICommand)obj.GetValue(DisplaySettingsChangedProperty);
        public static void SetDisplaySettingsChanged(DependencyObject obj, ICommand value) => obj.SetValue(DisplaySettingsChangedProperty, value);
        public static readonly DependencyProperty DisplaySettingsChangedProperty = DependencyProperty.RegisterAttached(
            "DisplaySettingsChanged", typeof(ICommand), typeof(FrameworkElementEx), new PropertyMetadata(null, OnDisplaySettingsChangedChanged));

        private static void OnDisplaySettingsChangedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (_, __) =>
            {
                // DisplaySettingsChanged has been observed to call back on a worker thread.
                dependencyObject.Dispatcher.BeginInvoke((Action)(() => ((ICommand)e.NewValue)?.Execute(null)));
            };
        }
    }
}
