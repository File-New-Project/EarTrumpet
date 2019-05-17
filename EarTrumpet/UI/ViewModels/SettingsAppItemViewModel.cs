using EarTrumpet.DataModel.Audio;
using EarTrumpet.Extensions;
using EarTrumpet.UI.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EarTrumpet.UI.ViewModels
{
    public class SettingsAppItemViewModel : BindableBase, IAppItemViewModel
    {
        public string Id { get; set; }
        public bool IsDesktopApp { get; set; }

        bool _isMuted;
        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (_isMuted != value)
                {
                    _isMuted = value;
                    RaisePropertyChanged(nameof(IsMuted));
                }
            }
        }

        int _volume;
        public int Volume
        {
            get => _volume;
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    RaisePropertyChanged(nameof(Volume));
                }
            }
        }

        public Color Background { get; set; }
        public ObservableCollection<IAppItemViewModel> ChildApps => null;
        public string DisplayName { get; set; }
        public string ExeName { get; set; }
        public string AppId { get; set; }
        public IconLoadInfo Icon { get; set; }
        public char IconText { get; set; }
        public bool IsExpanded => false;
        public bool IsMovable => false;
        public float PeakValue1 => 0;
        public float PeakValue2 => 0;
        public string PersistedOutputDevice => throw new NotImplementedException();
        public int ProcessId => throw new NotImplementedException();
        public IDeviceViewModel Parent => throw new NotImplementedException();

        public ICommand Remove { get; set; }

        public SettingsAppItemViewModel(IAudioDeviceSession session)
        {
            AppId = session.AppId;
            DisplayName = session.DisplayName;
            IsDesktopApp = session.IsDesktopApp;
            Id = session.AppId;
            Icon = new IconLoadInfo { IconPath = session.IconPath, IsDesktopApp = session.IsDesktopApp };
            Background = session.BackgroundColor.ToABGRColor();
        }

        public SettingsAppItemViewModel()
        {
        }

        public bool DoesGroupWith(IAppItemViewModel app)
        {
            throw new NotImplementedException();
        }

        public void MoveToDevice(string id, bool hide)
        {
            throw new NotImplementedException();
        }

        public void OpenPopup(FrameworkElement uIElement)
        {

        }

        public void UpdatePeakValueBackground()
        {
            throw new NotImplementedException();
        }

        public void UpdatePeakValueForeground()
        {
            throw new NotImplementedException();
        }
    }
}
