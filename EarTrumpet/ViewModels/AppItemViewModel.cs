using EarTrumpet.Extensions;
using EarTrumpet.Models;
using EarTrumpet.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EarTrumpet.ViewModels
{
    public class AppItemViewModel : BindableBase
    {
        private readonly IAudioMixerViewModelCallback _callback;
        private EarTrumpetAudioSessionModelGroup _sessions;

        public string DisplayName { get; set; }
        public uint SessionId { get; set; }
        public ImageSource Icon { get; set; }
        public double IconHeight { get; set; }
        public double IconWidth { get; set; }

        private int _volume = 0;
        public int Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                if (_volume != value)
                {
                    _volume = value;

                    foreach (var session in _sessions.Sessions)
                    {
                        _callback.SetVolume(session, (float)_volume / 100.0f);
                    }
                    RaisePropertyChanged("Volume");
                }
            }
        }
        public Color Background { get; set; }
        public bool IsDesktop { get; set; }

        public AppItemViewModel(IAudioMixerViewModelCallback callback, EarTrumpetAudioSessionModelGroup sessions)
        {
            _sessions = sessions;
            // select a session at random as sndvol does.
            var session = _sessions.Sessions.First();

            IconHeight = IconWidth = 32;
            SessionId = session.SessionId;
            DisplayName = session.DisplayName;
            IsDesktop = session.IsDesktop;

            _volume = Convert.ToInt32(Math.Round((session.Volume * 100),
                                     MidpointRounding.AwayFromZero));
            _callback = callback;

            if (session.IsDesktop)
            {                
                try
                {
                    if (Path.GetExtension(session.IconPath) == ".dll")
                    {
                        Icon = IconExtensions.ExtractIconFromDll(session.IconPath);
                    }
                    else
                    {
                        Icon = System.Drawing.Icon.ExtractAssociatedIcon(session.IconPath).ToImageSource();
                    }
                }
                catch { }

                Background = Colors.Transparent;

                try
                {
                    var proc = Process.GetProcessById((int)session.ProcessId);
                    if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                    {
                        DisplayName = proc.MainWindowTitle;
                    }
                }
                catch { } // we fallback to exe name if DisplayName is not set in the try above.
            }
            else
            {
                Icon = new BitmapImage(new Uri(session.IconPath));
                Background = AccentColorService.FromABGR(session.BackgroundColor);
            }
        }

        public void UpdateFromOther(AppItemViewModel other)
        {
            if (_volume != other.Volume)
            {
                _sessions = other._sessions;
                _volume = other.Volume;
                RaisePropertyChanged("Volume");
            }
        }

        public bool IsSame(AppItemViewModel other)
        {
            foreach (var session in other._sessions.Sessions)
            {
                if (_sessions.Sessions.Where(x => x.SessionId == session.SessionId).Any())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
