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
using System.Windows.Threading;

namespace EarTrumpet.ViewModels
{
    public class AppItemViewModel : BindableBase
    {
        private IAudioDeviceSession _session;

        private string displayName;
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(displayName))
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var proc = Process.GetProcessById((int)ProcessId);
                            if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                            {
                                displayName = proc.MainWindowTitle;
                                App.Current.Dispatcher.BeginInvoke((Action)delegate { RaisePropertyChanged("DisplayName"); }, DispatcherPriority.Background);
                            }
                        }
                        catch { } // we fallback to exe name if DisplayName is not set in the try above.                 
                    });
                    return ExeName;
                }
                return displayName;
            }            
        }
        public string ExeName { get; private set; }
        public string SessionId { get; private set; }
        public uint ProcessId { get; set; }
        public ImageSource Icon { get; set; }
        public double IconHeight { get; set; }
        public double IconWidth { get; set; }

        public int Volume
        {
            get
            {
                return _session.Volume.ToVolumeInt();
            }
            set
            {
                _session.Volume = value / 100f;
                RaisePropertyChanged("Volume");
            }
        }
        public SolidColorBrush Background { get; set; }
        public bool IsDesktop { get; set; }

        public bool IsMuted
        {
            get
            {
                return _session.IsMuted;
            }
            set
            {
                if (IsMuted != value)
                {
                    _session.IsMuted = value;
                    RaisePropertyChanged("IsMuted");
                }
            }
        }

        public char IconText
        {
            get
            {
                return DisplayName.ToUpperInvariant().FirstOrDefault(x => char.IsLetterOrDigit(x));
            }
        }

        public float PeakValue => _session.PeakValue;

        public void TriggerPeakCheck()
        {
            RaisePropertyChanged("PeakValue");
        }

        public AppItemViewModel(IAudioDeviceSession session)
        {
            _session = session;
            _session.PropertyChanged += Session_PropertyChanged;

            IconHeight = IconWidth = 24;
            SessionId = session.Id;
            ProcessId = (uint)session.ProcessId;
            IsDesktop = session.IsDesktopApp;
            ExeName = session.DisplayName;

            if (session.DisplayName.ToLowerInvariant() == "speechruntime.exe")
            {
                ExeName = Properties.Resources.SpeechRuntimeDisplayName;
            }
            else if (session.IsSystemSoundsSession)
            {
                ExeName = Properties.Resources.SystemSoundsDisplayName;
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
                if (Icon == null)
                {
                    Background = new SolidColorBrush(AccentColorService.GetColorByTypeName("ImmersiveSystemAccent"));
                }
                else
                {
                    Background = new SolidColorBrush(Colors.Transparent);
                }
            }
            else
            {
                if (File.Exists(session.IconPath)) //hack until we invoke the resource manager correctly.
                {                    
                    Icon = new BitmapImage(new Uri(session.IconPath));
                }
                Background = new SolidColorBrush(AccentColorService.FromABGR(session.BackgroundColor));
            }
        }

        private void Session_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Volume" ||
                e.PropertyName == "IsMuted")
            {
                RaisePropertyChanged(e.PropertyName);
            }
        }
    }
}
