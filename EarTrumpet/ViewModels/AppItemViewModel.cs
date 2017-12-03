using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using EarTrumpet.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarTrumpet.ViewModels
{
    public class AppItemViewModel : VolumeControlChannelViewModel
    {
        private IAudioDeviceSession _session;

        public string ExeName { get; private set; }

        public ImageSource Icon { get; private set; }

        public SolidColorBrush Background { get; private set; }

        public char IconText => DisplayName != null ? DisplayName.ToUpperInvariant().FirstOrDefault(x => char.IsLetterOrDigit(x)) : 'X';

        string _displayName;
        public override string DisplayName => _displayName;

        public AppItemViewModel(IAudioDeviceSession session) : base(session)
        {
            _session = session;

            ExeName = session.DisplayName;

            if (session.DisplayName.ToLowerInvariant() == "speechruntime.exe")
            {
                _displayName = Properties.Resources.SpeechRuntimeDisplayName;
            }
            else if (session.IsSystemSoundsSession)
            {
                _displayName = Properties.Resources.SystemSoundsDisplayName;
            }

            if (session.IsDesktopApp)
            {                
                try
                {
                    if (Path.GetExtension(session.IconPath) == ".dll")
                    {
                        Icon = IconService.GetIconFromFileAsImageSource(session.IconPath);
                    }
                    else
                    {
                        // override for SpeechRuntime.exe (Repo -> HEY CORTANA)
                        if (session.IconPath.ToLowerInvariant().Contains("speechruntime.exe"))
                        {
                            var sysType = Environment.Is64BitOperatingSystem ? "SysNative" : "System32";
                            Icon = IconService.GetIconFromFileAsImageSource(Path.Combine("%windir%", sysType, "Speech\\SpeechUX\\SpeechUXWiz.exe"), 0);
                        }
                        else
                        {
                            Icon = System.Drawing.Icon.ExtractAssociatedIcon(session.IconPath).ToImageSource();
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                Background = new SolidColorBrush(Icon == null ? 
                    AccentColorService.GetColorByTypeName("ImmersiveSystemAccent") :
                    Colors.Transparent);
            }
            else
            {
                if (File.Exists(session.IconPath)) // hack until we invoke the resource manager correctly.
                {                    
                    Icon = new BitmapImage(new Uri(session.IconPath));
                }
                Background = new SolidColorBrush(AccentColorService.FromABGR(session.BackgroundColor));
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var proc = Process.GetProcessById((int)_session.ProcessId);
                    if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                    {
                        var displayName = proc.MainWindowTitle;
                        App.Current.Dispatcher.SafeInvoke(() =>
                        {
                            _displayName = displayName;
                            RaisePropertyChanged("DisplayName");
                        });
                    }
                }
                catch { } // we fallback to exe name if DisplayName is not set in the try above.                 
            });
        }
    }
}
