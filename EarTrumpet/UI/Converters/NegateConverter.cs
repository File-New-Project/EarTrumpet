using System;
using System.Windows.Data;

namespace EarTrumpet.UI.Converters
{
    public class NegateConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b) return !b;
            if (value is double d) return -d;
            if (value is float f) return (double)-f;
            if (value is int i) return (double)-i;
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b) return !b;
            if (value is double d) return -d;
            throw new InvalidOperationException();
        }
    }
}