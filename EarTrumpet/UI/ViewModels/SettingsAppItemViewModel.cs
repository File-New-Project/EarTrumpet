using EarTrumpet.DataModel.Audio;
using EarTrumpet.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EarTrumpet.UI.ViewModels;

public class SettingsAppItemViewModel : BindableBase, IAppItemViewModel
{
    public string Id { get; set; }
    public bool IsDesktopApp { get; set; }

    private bool _isMuted;
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

    private int _volume;
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
    public char IconText { get; set; }
    public string IconPath { get; set; }
    public bool IsExpanded => false;
    public bool IsMovable => false;
    public float PeakValue1 => 0;
    public float PeakValue2 => 0;
    public string PersistedOutputDevice => throw new NotImplementedException();
    public uint ProcessId => throw new NotImplementedException();
    public IDeviceViewModel Parent => throw new NotImplementedException();
    public ICommand Remove { get; set; }
    public string PackageInstallPath { get; set; }

    public SettingsAppItemViewModel(IAudioDeviceSession session)
    {
        AppId = session.AppId;
        PackageInstallPath = session.PackageInstallPath;
        DisplayName = session.DisplayName;
        IsDesktopApp = session.IsDesktopApp;
        IconPath = session.IconPath;
        Id = session.AppId;
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

    public static void OpenPopup(FrameworkElement uIElement)
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
