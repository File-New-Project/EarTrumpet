using EarTrumpet.DataModel;
using System.Diagnostics;
using System.Windows;

namespace EarTrumpet.UI.Behaviors
{
    public class FrameworkElementEx
    {
        public enum FlowDirectionKind
        {
            Auto,
            Unset
        }

        public static FlowDirectionKind GetFlowDirection(DependencyObject obj) => (FlowDirectionKind)obj.GetValue(FlowDirectionProperty);
        public static void SetFlowDirection(DependencyObject obj, FlowDirectionKind value) => obj.SetValue(FlowDirectionProperty, value);
        public static readonly DependencyProperty FlowDirectionProperty =DependencyProperty.RegisterAttached(
            "FlowDirection", typeof(FlowDirectionKind), typeof(FrameworkElementEx), new PropertyMetadata(FlowDirectionKind.Unset, OnFlowDirectionChanged));

        private static void OnFlowDirectionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert((FlowDirectionKind)e.NewValue == FlowDirectionKind.Auto);
            ((FrameworkElement)dependencyObject).FlowDirection = SystemSettings.IsRTL ? 
                System.Windows.FlowDirection.RightToLeft : System.Windows.FlowDirection.LeftToRight;
        }
    }
}
