using EarTrumpet.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace EarTrumpet.Views
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
