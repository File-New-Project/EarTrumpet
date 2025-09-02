using System;
using System.Windows.Data;

namespace EarTrumpet.UI.Converters;

public class VolumeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is float vol)
        {
            if (App.Settings.UseLogarithmicVolume)
            {
                // Special case for -0.0 display
                if (vol >= -0.05)
                {
                    return "-0.0";
                }
                return $"{vol:0.0}";
            }
            return vol.ToString();
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}