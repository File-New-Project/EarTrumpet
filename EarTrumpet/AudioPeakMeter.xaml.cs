using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EarTrumpet
{
    /// <summary>
    /// Interaction logic for AudioPeakMeter.xaml
    /// </summary>
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
