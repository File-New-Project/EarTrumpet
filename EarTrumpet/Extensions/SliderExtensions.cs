using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet.Extensions
{
    public static class SliderExtensions
    {
        public static void SetPositionByControlPoint(this Slider slider, Point point)
        {
            var percent = point.X / slider.ActualWidth;
            var newValue = (slider.Maximum - slider.Minimum) * percent;
            slider.Value = newValue.Bound(slider.Minimum,slider.Maximum);
        }

        public static void ChangePositionByAmount(this Slider slider, double amount)
        {
            slider.Value = (slider.Value + amount).Bound(slider.Minimum, slider.Maximum);
        }
    }
}
