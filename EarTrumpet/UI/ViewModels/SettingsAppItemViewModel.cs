using EarTrumpet.DataModel;
using EarTrumpet.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace EarTrumpet.UI.ViewModels
{
    public class SettingsAppItemViewModel : BindableBase, IAppItemViewModel
    {
        public string Id { get; set; }
        public bool IsDesktopApp { get; set; }
        public bool IsMuted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Color Background { get; set; }
        public ObservableCollection<IAppItemViewModel> ChildApps => null;
        public string DisplayName { get; set; }
        public string ExeName { get; set; }
        public string AppId { get; set; }
        public IconLoadInfo Icon { get; set; }
        public char IconText { get; set; }
        public bool IsExpanded => false;
        public bool IsMovable => false;
        public float PeakValue1 => throw new NotImplementedException();
        public float PeakValue2 => throw new NotImplementedException();
        public string PersistedOutputDevice => throw new NotImplementedException();
        public int ProcessId => throw new NotImplementedException();
        public IDeviceViewModel Parent => throw new NotImplementedException();

        public SettingsAppItemViewModel(IAudioDeviceSession session)
        {
            DisplayName = session.SessionDisplayName;
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

        public void RefreshDisplayName()
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
