using System;

namespace EarTrumpet.UI.ViewModels;

public class EarTrumpetCommunitySettingsPageViewModel : SettingsPageViewModel
{
    private readonly AppSettings _settings;
    public bool UseLogarithmicVolume
    {
        get => _settings.UseLogarithmicVolume;
        set
        {
            if (_settings.UseLogarithmicVolume != value)
            {
                _settings.UseLogarithmicVolume = value;
                RaisePropertyChanged(nameof(UseLogarithmicVolume));
            }
        }
    }

    public double LogarithmicVolumeMinDb
    {
        get => _settings.LogarithmicVolumeMinDb;
        set
        {
            if (_settings.LogarithmicVolumeMinDb != (float)value)
            {
                _settings.LogarithmicVolumeMinDb = (float)value;
                RaisePropertyChanged(nameof(LogarithmicVolumeMinDb));
            }
        }
    }

    public double ScrollWheelOrHotkeyVolumeChangePercent
    {
        get => _settings.ScrollWheelOrHotkeyVolumeChangePercent;
        set
        {
            if (_settings.ScrollWheelOrHotkeyVolumeChangePercent != (float)value)
            {
                _settings.ScrollWheelOrHotkeyVolumeChangePercent = (int)Math.Round(value);
                RaisePropertyChanged(nameof(ScrollWheelOrHotkeyVolumeChangePercent));
            }
        }
    }

    public double ScrollWheelOrHotkeyVolumeChangeDb
    {
        get => _settings.ScrollWheelOrHotkeyVolumeChangeDb;
        set
        {
            if (_settings.ScrollWheelOrHotkeyVolumeChangeDb != (float)value)
            {
                _settings.ScrollWheelOrHotkeyVolumeChangeDb = (float)value;
                RaisePropertyChanged(nameof(ScrollWheelOrHotkeyVolumeChangeDb));
            }
        }
    }

    public bool ShowFullMixerWindowOnStartup
    {
        get => _settings.ShowFullMixerWindowOnStartup;
        set => _settings.ShowFullMixerWindowOnStartup = value;
    }

    public EarTrumpetCommunitySettingsPageViewModel(AppSettings settings) : base(null)
    {
        _settings = settings;
        Title = Properties.Resources.CommunitySettingsPageText;
        Glyph = "\xE902";
    }
}