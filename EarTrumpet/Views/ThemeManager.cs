using EarTrumpet.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace EarTrumpet.Views
{
    public class ThemeManager : INotifyPropertyChanged
    {
        private class Window : System.Windows.Forms.NativeWindow, IDisposable
        {
            Action<System.Windows.Forms.Message> _wndProc;

            public void Initialize(Action<System.Windows.Forms.Message> wndProc)
            {
                _wndProc = wndProc;

                CreateHandle(new System.Windows.Forms.CreateParams());
            }

            protected override void WndProc(ref System.Windows.Forms.Message m)
            {
                _wndProc(m);

                base.WndProc(ref m);
            }

            public void Dispose()
            {
                this.DestroyHandle();
            }
        }

        public interface IResolvableThemeBrush
        {
            Color Resolve(ThemeResolveData data);
        }

        public class ThemeResolveData
        {
            public bool IsHighContrast = SystemParameters.HighContrast;
            public bool IsTransparencyEnabled = SystemSettings.IsTransparencyEnabled;
            public bool IsLightTheme = SystemSettings.IsLightTheme;
            public bool UseAccentColor = SystemSettings.UseAccentColor;
            public Color LookupThemeColor(string color) => ImmersiveSystemColors.Lookup(color);
        }

        public event Action ThemeChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool AnimationsEnabled => SystemParameters.MenuAnimation;
        public bool IsLightTheme => SystemSettings.IsLightTheme;

        private Dictionary<string, IResolvableThemeBrush> _themeData;
        private DispatcherTimer _themeChangeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        private Window _messageWindow;

        public ThemeManager()
        {
            _themeChangeTimer.Tick += ThemeChangeTimer_Tick;
        }

        ~ThemeManager()
        {
            _themeChangeTimer.Tick -= ThemeChangeTimer_Tick;
        }

        public void SetTheme(Dictionary<string, ThemeManager.IResolvableThemeBrush> data)
        {
            _themeData = data;

            _messageWindow = new Window();
            _messageWindow.Initialize((m) => WndProc(m.Msg, m.WParam, m.LParam));

            RebuildTheme();
        }

        private void RebuildTheme()
        {
            Trace.WriteLine("ThemeManager RebuildTheme");

            var oldDictionary = Application.Current.Resources.MergedDictionaries[0];

            var resolveData = new ThemeResolveData();
            var newDictionary = new ResourceDictionary();
            foreach (var themeEntry in _themeData)
            {
                newDictionary[themeEntry.Key] = new SolidColorBrush(themeEntry.Value.Resolve(resolveData));

#if DEBUG
                // Verify the old dictionary has this entry. i.e. fix SystemThemeColors.xaml
                var oldEntry = oldDictionary[themeEntry.Key];
                if (oldEntry == null)
                {
                    throw new InvalidOperationException($"{themeEntry.Key} is missing from the previous dictionary");
                }
#endif
            }

#if DEBUG
            foreach (var key in oldDictionary.Keys)
            {
                // Verify the new diction has the old entry. i.e. fix ThemeData.cs
                var newEntry = newDictionary[key];
                if (newEntry == null)
                {
                    throw new InvalidOperationException($"{key} is missing from the new dictionary");
                }
            }
#endif

            Application.Current.Resources.MergedDictionaries.RemoveAt(0);
            Application.Current.Resources.MergedDictionaries.Insert(0, newDictionary);
        }

        private void WndProc(int msg, IntPtr wParam, IntPtr lParam)
        {
            const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x320;
            const int WM_DWMCOMPOSITIONCHANGED = 0x31E;
            const int WM_THEMECHANGED = 0x031A;
            const int WM_SETTINGCHANGE = 0x001A;

            switch (msg)
            {
                case WM_DWMCOLORIZATIONCOLORCHANGED:
                case WM_DWMCOMPOSITIONCHANGED:
                case WM_THEMECHANGED:
                    OnThemeColorsChanged();
                    break;
                case WM_SETTINGCHANGE:
                    var settingChanged = Marshal.PtrToStringUni(lParam);
                    if (settingChanged == "ImmersiveColorSet")
                    {
                        OnThemeColorsChanged();
                    }
                    else if (settingChanged == "WindowMetrics")
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AnimationsEnabled)));
                    }
                    break;
            }
        }

        private void OnThemeColorsChanged()
        {
            if (_themeChangeTimer.IsEnabled)
            {
                _themeChangeTimer.Stop();
            }
            _themeChangeTimer.Start();
        }

        private void ThemeChangeTimer_Tick(object sender, EventArgs e)
        {
            _themeChangeTimer.IsEnabled = false;

            RebuildTheme();

            ThemeChanged?.Invoke();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLightTheme)));
        }
    }
}