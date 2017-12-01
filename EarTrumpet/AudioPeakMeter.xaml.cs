using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet
{
    public partial class AudioPeakMeter : UserControl
    {
        public float PeakValue
        {
            get { return (float)this.GetValue(PeakValueProperty); }
            set { this.SetValue(PeakValueProperty, value); }
        }
        public static readonly DependencyProperty PeakValueProperty = DependencyProperty.Register(
          "PeakValue", typeof(float), typeof(AudioPeakMeter), new PropertyMetadata(0f, new PropertyChangedCallback(PeakValueChanged)));

        public AudioPeakMeter()
        {
            InitializeComponent();

            peakBorder.Width = 0;
        }

        private static void PeakValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AudioPeakMeter)d).PeakValueChanged();
        }
        private void PeakValueChanged()
        {
            peakBorder.Width = mainGrid.ActualWidth * PeakValue;
        }
    }
}
