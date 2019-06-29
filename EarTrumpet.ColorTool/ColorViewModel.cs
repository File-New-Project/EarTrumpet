using EarTrumpet.Interop.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EarTrumpet.ColorTool
{
    public class ColorViewModel : BindableBase
    {
        public IEnumerable<ColorItemViewModel> Colors { get; private set; }

        public ColorViewModel()
        {
            Refresh();
        }

        public void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Refresh(((TextBox)sender).Text);
        }

        public void Refresh(string search = null)
        {
            var colors = new List<ColorItemViewModel>();

            if (SystemParameters.HighContrast)
            {
                colors.Add(new ColorItemViewModel { Color = SystemColors.ActiveBorderBrush, Name = "ActiveBorderBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.ActiveCaptionBrush, Name = "ActiveCaptionBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.AppWorkspaceBrush, Name = "AppWorkspaceBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.ControlBrush, Name = "ControlBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.ControlDarkBrush, Name = "ControlDarkBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.ControlDarkDarkBrush, Name = "ControlDarkDarkBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.ControlLightBrush, Name = "ControlLightBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.ControlLightLightBrush, Name = "ControlLightLightBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.ControlTextBrush, Name = "ControlTextBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.DesktopBrush, Name = "DesktopBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.GradientActiveCaptionBrush, Name = "GradientActiveCaptionBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.GradientInactiveCaptionBrush, Name = "GradientInactiveCaptionBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.GrayTextBrush, Name = "GrayTextBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.HighlightBrush, Name = "HighlightBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.HighlightTextBrush, Name = "HighlightTextBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.HotTrackBrush, Name = "HotTrackBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.InactiveBorderBrush, Name = "InactiveBorderBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.InactiveCaptionBrush, Name = "InactiveCaptionBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.InactiveSelectionHighlightBrush, Name = "InactiveSelectionHighlightBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.InactiveSelectionHighlightTextBrush, Name = "InactiveSelectionHighlightTextBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.InfoBrush, Name = "InfoBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.InfoTextBrush, Name = "InfoTextBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.MenuBarBrush, Name = "MenuBarBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.MenuBrush, Name = "MenuBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.MenuHighlightBrush, Name = "MenuHighlightBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.MenuTextBrush, Name = "MenuTextBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.ScrollBarBrush, Name = "ScrollBarBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.WindowBrush, Name = "WindowBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.WindowFrameBrush, Name = "WindowFrameBrush" });
                colors.Add(new ColorItemViewModel { Color = SystemColors.WindowTextBrush, Name = "WindowTextBrush" });
            }
            else
            {
                foreach (var c in ImmersiveSystemColors.GetList().OrderBy(d => d.Key))
                {
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        if (!c.Key.ToLower().Contains(search.ToLower())) continue;
                    }

                    // Exclude junk UI-specific colors that are stale anyway.
                    if (c.Key.Contains("Boot")) continue;
                    if (c.Key.Contains("Start")) continue;
                    if (c.Key.Contains("Hardware")) continue;
                    if (c.Key.Contains("Files")) continue;
                    if (c.Key.Contains("Multitasking")) continue;

                    var color = c.Value;
                    // color.A = 255; // Very misleading to render on white otherwise!
                    colors.Add(new ColorItemViewModel { Color = new SolidColorBrush(color), Opacity = $"{Math.Round(((float)color.A / 255) * 100, 0)}%", Name = c.Key });
                }
            }

            Colors = colors;
            RaisePropertyChanged(nameof(Colors));
        }
    }
}
