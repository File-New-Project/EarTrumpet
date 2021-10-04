using EarTrumpet.Extensions;
using System;
using System.Windows;

namespace EarTrumpet.UI.Themes
{
    public class OS : DependencyObject
    {
        public static readonly DependencyPropertyKey IsWindows11PropertyKey =
            DependencyProperty.RegisterReadOnly("IsWindows11", typeof(bool), typeof(OS), new PropertyMetadata(Environment.OSVersion.IsAtLeast(OSVersions.Windows11)));

        public bool IsWindows11 => (bool)GetValue(IsWindows11PropertyKey.DependencyProperty);
    }
}
