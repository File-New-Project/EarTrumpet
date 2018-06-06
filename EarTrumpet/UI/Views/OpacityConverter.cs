using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EarTrumpet.UI.Views
{
    public class OpacityConverter : DependencyObject, IValueConverter
    {
        public float TrueOpacity
        {
            get { return (float)this.GetValue(TrueOpacityProperty); }
            set { this.SetValue(TrueOpacityProperty, value); }
        }
        public static readonly DependencyProperty TrueOpacityProperty = DependencyProperty.Register(
          "TrueOpacity", typeof(float), typeof(OpacityConverter), new PropertyMetadata());

        public float FalseOpacity
        {
            get { return (float)this.GetValue(FalseOpacityProperty); }
            set { this.SetValue(FalseOpacityProperty, value); }
        }
        public static readonly DependencyProperty FalseOpacityProperty = DependencyProperty.Register(
          "FalseOpacity", typeof(float), typeof(OpacityConverter), new PropertyMetadata());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? TrueOpacity : FalseOpacity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
