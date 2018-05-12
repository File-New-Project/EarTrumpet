using EarTrumpet.Services;
using System.Windows;
using System.Windows.Controls;

namespace EarTrumpet.UserControls
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

        public int Volume
        {
            get { return (int)this.GetValue(VolumeValueProperty); }
            set { this.SetValue(VolumeValueProperty, value); }
        }
        public static readonly DependencyProperty VolumeValueProperty = DependencyProperty.Register(
          "Volume", typeof(int), typeof(AudioPeakMeter), new PropertyMetadata(0, new PropertyChangedCallback(VolumeChanged)));


        public AudioPeakMeter()
        {
            InitializeComponent();

            peakBorder.Width = 0;

            SizeOrVolumeOrPeakValueChanged();
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var ret = base.ArrangeOverride(arrangeBounds);
            SizeOrVolumeOrPeakValueChanged();
            return ret;
        }

        private static void PeakValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AudioPeakMeter)d).SizeOrVolumeOrPeakValueChanged();
        }

        private void SizeOrVolumeOrPeakValueChanged()
        {
            volumeBorder.Width = MainGrid.ActualWidth * (Volume / 100f);
            peakBorder.Width = MainGrid.ActualWidth * PeakValue * (Volume/100f);
        }

        private static void VolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AudioPeakMeter)d).SizeOrVolumeOrPeakValueChanged();
        }
    }
}
