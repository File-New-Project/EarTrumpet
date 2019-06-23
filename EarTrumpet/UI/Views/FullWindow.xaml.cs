using EarTrumpet.Extensions;
using EarTrumpet.Interop.Helpers;
using System;
using System.Diagnostics;
using System.Windows;

namespace EarTrumpet.UI.Views
{
    public partial class FullWindow : Window
    {
        public FullWindow()
        {
            Trace.WriteLine("FullWindow .ctor");
            Closed += (_, __) => Trace.WriteLine("FullWindow Closed");

            InitializeComponent();

            SourceInitialized += OnSourceInitialized;
            Themes.Manager.Current.ThemeChanged += SetBlurColor;
        }

        private void SetBlurColor()
        {
            AccentPolicyLibrary.EnableAcrylic(this, Themes.Manager.Current.ResolveRef(this, "AcrylicColor_Settings"), Interop.User32.AccentFlags.DrawAllBorders);
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            this.Cloak();
            SetBlurColor();
        }
    }
}
