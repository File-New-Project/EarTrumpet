using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShowImmersiveColors
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            SetColors();
        }

        private void SetColors()
        {

            List<ColorData> colors = new List<ColorData>();

            if (SystemParameters.HighContrast)
            {
                colors.Add(new ColorData { Color = SystemColors.ActiveBorderBrush, Name = "ActiveBorderBrush" });
                colors.Add(new ColorData { Color = SystemColors.ActiveCaptionBrush, Name = "ActiveCaptionBrush" });
                colors.Add(new ColorData { Color = SystemColors.AppWorkspaceBrush, Name = "AppWorkspaceBrush" });
                colors.Add(new ColorData { Color = SystemColors.ControlBrush, Name = "ControlBrush" });
                colors.Add(new ColorData { Color = SystemColors.ControlDarkBrush, Name = "ControlDarkBrush" });
                colors.Add(new ColorData { Color = SystemColors.ControlDarkDarkBrush, Name = "ControlDarkDarkBrush" });
                colors.Add(new ColorData { Color = SystemColors.ControlLightBrush, Name = "ControlLightBrush" });
                colors.Add(new ColorData { Color = SystemColors.ControlLightLightBrush, Name = "ControlLightLightBrush" });
                colors.Add(new ColorData { Color = SystemColors.ControlTextBrush, Name = "ControlTextBrush" });
                colors.Add(new ColorData { Color = SystemColors.DesktopBrush, Name = "DesktopBrush" });
                colors.Add(new ColorData { Color = SystemColors.GradientActiveCaptionBrush, Name = "GradientActiveCaptionBrush" });
                colors.Add(new ColorData { Color = SystemColors.GradientInactiveCaptionBrush, Name = "GradientInactiveCaptionBrush" });
                colors.Add(new ColorData { Color = SystemColors.GrayTextBrush, Name = "GrayTextBrush" });
                colors.Add(new ColorData { Color = SystemColors.HighlightBrush, Name = "HighlightBrush" });
                colors.Add(new ColorData { Color = SystemColors.HighlightTextBrush, Name = "HighlightTextBrush" });
                colors.Add(new ColorData { Color = SystemColors.HotTrackBrush, Name = "HotTrackBrush" });
                colors.Add(new ColorData { Color = SystemColors.InactiveBorderBrush, Name = "InactiveBorderBrush" });
                colors.Add(new ColorData { Color = SystemColors.InactiveCaptionBrush, Name = "InactiveCaptionBrush" });
                colors.Add(new ColorData { Color = SystemColors.InactiveSelectionHighlightBrush, Name = "InactiveSelectionHighlightBrush" });
                colors.Add(new ColorData { Color = SystemColors.InactiveSelectionHighlightTextBrush, Name = "InactiveSelectionHighlightTextBrush" });
                colors.Add(new ColorData { Color = SystemColors.InfoBrush, Name = "InfoBrush" });
                colors.Add(new ColorData { Color = SystemColors.InfoTextBrush, Name = "InfoTextBrush" });
                colors.Add(new ColorData { Color = SystemColors.MenuBarBrush, Name = "MenuBarBrush" });
                colors.Add(new ColorData { Color = SystemColors.MenuBrush, Name = "MenuBrush" });
                colors.Add(new ColorData { Color = SystemColors.MenuHighlightBrush, Name = "MenuHighlightBrush" });
                colors.Add(new ColorData { Color = SystemColors.MenuTextBrush, Name = "MenuTextBrush" });
                colors.Add(new ColorData { Color = SystemColors.ScrollBarBrush, Name = "ScrollBarBrush" });
                colors.Add(new ColorData { Color = SystemColors.WindowBrush, Name = "WindowBrush" });
                colors.Add(new ColorData { Color = SystemColors.WindowFrameBrush, Name = "WindowFrameBrush" });
                colors.Add(new ColorData { Color = SystemColors.WindowTextBrush, Name = "WindowTextBrush" });
            }
            else
            {

                foreach (var c in ImmersiveSystemColors.GetList().OrderBy(d => d.Key))
                {
                    if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                    {
                        if (!c.Key.ToLower().Contains(txtSearch.Text.ToLower())) continue;
                    }
                    if (c.Key.Contains("Boot")) continue;
                    if (c.Key.Contains("Start")) continue;
                    if (c.Key.Contains("Hardware")) continue;
                    if (c.Key.Contains("Files")) continue;
                    if (c.Key.Contains("Multitasking")) continue;
                    var color = c.Value;
                    // color.A = 255; // Very misleading to render on white otherwise!
                    colors.Add(new ColorData { Color = new SolidColorBrush(color), Opacity = $"{Math.Round(((float)color.A / 255) * 100, 0)}%", Name = c.Key });
                }
            }

            Colors.ItemsSource = colors;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetColors();
        }
    }

    public class ColorData
    {
        public ColorData() { }

        public string Name { get; set; }
        public string Opacity { get; set; }
        public SolidColorBrush Color { get; set; }
    }

    class ImmersiveSystemColors
    {
        internal static Color Lookup(string name)
        {
            var colorSet = Uxtheme.GetImmersiveUserColorSetPreference(false, false);
            var colorType = Uxtheme.GetImmersiveColorTypeFromName(name);
            var rawColor = Uxtheme.GetImmersiveColorFromColorSetEx(colorSet, colorType, false, 0);

            return rawColor.ToABGRColor();
        }

        internal static IDictionary<string, Color> GetList()
        {
            var colors = new Dictionary<string, Color>();
            var colorSet = Uxtheme.GetImmersiveUserColorSetPreference(false, false);

            for (uint i = 0; ; i++)
            {
                var ptr = Uxtheme.GetImmersiveColorNamedTypeByIndex(i);
                if (ptr == IntPtr.Zero)
                    break;

                var name = Marshal.PtrToStringUni(Marshal.ReadIntPtr(ptr));
                colors.Add(name, Lookup($"Immersive{name}"));
            }

            return colors;
        }
    }

    class Uxtheme
    {
        [DllImport("uxtheme.dll", EntryPoint = "#94", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern int GetImmersiveColorSetCount();

        [DllImport("uxtheme.dll", EntryPoint = "#95", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern uint GetImmersiveColorFromColorSetEx(
            uint dwImmersiveColorSet,
            uint dwImmersiveColorType,
            bool bIgnoreHighContrast,
            uint dwHighContrastCacheMode);

        [DllImport("uxtheme.dll", EntryPoint = "#96", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern uint GetImmersiveColorTypeFromName(
            string name);

        [DllImport("uxtheme.dll", EntryPoint = "#98", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern uint GetImmersiveUserColorSetPreference(
            bool bForceCheckRegistry,
            bool bSkipCheckOnFail);

        [DllImport("uxtheme.dll", EntryPoint = "#100", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern IntPtr GetImmersiveColorNamedTypeByIndex(
            uint dwIndex);
    }

    public static class uintColorExtensions
    {
        public static Color ToABGRColor(this uint abgrValue)
        {
            var colorBytes = new byte[4];
            colorBytes[0] = (byte)((0xFF000000 & abgrValue) >> 24);     // A
            colorBytes[1] = (byte)((0x00FF0000 & abgrValue) >> 16);     // B
            colorBytes[2] = (byte)((0x0000FF00 & abgrValue) >> 8);      // G
            colorBytes[3] = (byte)(0x000000FF & abgrValue);             // R

            return Color.FromArgb(colorBytes[0], colorBytes[3], colorBytes[2], colorBytes[1]);
        }
    }
}
