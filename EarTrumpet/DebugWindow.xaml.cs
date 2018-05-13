using EarTrumpet.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace EarTrumpet
{
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();

            List<ColorData> colors = new List<ColorData>();
            foreach(var c in AccentColorService.GetImmersiveColors())
            {
                colors.Add(new ColorData { Color = new SolidColorBrush(c.Value), Name = c.Key });
            }

            Colors.ItemsSource = colors;
        }
    }

    public class ColorData
    {
        public ColorData() { }

        public string Name { get; set; }
        public SolidColorBrush Color { get; set; }
    }
}
