using EarTrumpet.Extensions;
using EarTrumpet.Models;
using EarTrumpet.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace EarTrumpet.ViewModels
{
    public class AppItemViewModel : BindableBase
    {
        private readonly IAudioMixerViewModelCallback _callback;
        private EarTrumpetAudioSessionModelGroup _sessions;

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
        public uint SessionId { get; private set; }
        public uint ProcessId { get; set; }
        public ImageSource Icon { get; set; }
        public double IconHeight { get; set; }
        public double IconWidth { get; set; }

        private int _volume;
        public int Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                if (_volume == value) return;
                _volume = value;

                foreach (var session in _sessions.Sessions)
                {
                    _callback.SetVolume(session, _volume / 100.0f);
                }
                RaisePropertyChanged("Volume");
            }
        }
        public SolidColorBrush Background { get; set; }
        public bool IsDesktop { get; set; }

        public AppItemViewModel(IAudioMixerViewModelCallback callback, EarTrumpetAudioSessionModelGroup sessions)
        {
            _sessions = sessions;
            // select a session at random as sndvol does.
            var session = _sessions.Sessions.First();

            IconHeight = IconWidth = 32;
            SessionId = session.SessionId;
            ProcessId = session.ProcessId;
            ExeName = session.DisplayName.Equals("System Sounds") ? EarTrumpet.Properties.Resources.SystemSoundsDisplayName : session.DisplayName;
            IsDesktop = session.IsDesktop;

            _volume = Convert.ToInt32(Math.Round((session.Volume * 100),
                                     MidpointRounding.AwayFromZero));
            _callback = callback;

            if (session.IsDesktop)
            {                
                try
                {
                    Icon = Path.GetExtension(session.IconPath) == ".dll" ? IconExtensions.ExtractIconFromDll(session.IconPath) : System.Drawing.Icon.ExtractAssociatedIcon(session.IconPath).ToImageSource();
                }
                catch
                {
                    // ignored
                }
                Background = new SolidColorBrush(Colors.Transparent);
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

        public void UpdateFromOther(AppItemViewModel other)
        {
            if (_volume == other.Volume) return;
            _sessions = other._sessions;
            _volume = other.Volume;
            RaisePropertyChanged("Volume");
        }

        public bool IsSame(AppItemViewModel other)
        {
            return other._sessions.Sessions.Any(session => _sessions.Sessions.Any(x => x.SessionId == session.SessionId));
        }
    }
}
