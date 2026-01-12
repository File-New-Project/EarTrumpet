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

    public bool ShowFullMixerWindowOnStartup
    {
        get => _settings.ShowFullMixerWindowOnStartup;
        set => _settings.ShowFullMixerWindowOnStartup = value;
    }

    public int VolumeStepAmount
    {
        get => _settings.VolumeStepAmount;
        set
        {
            if (_settings.VolumeStepAmount != value)
            {
                _settings.VolumeStepAmount = value;
                RaisePropertyChanged(nameof(VolumeStepAmount));
            }
        }
    }

    public bool UseRangeSnapping
    {
        get => _settings.UseRangeSnapping;
        set
        {
            if (_settings.UseRangeSnapping != value)
            {
                _settings.UseRangeSnapping = value;
                RaisePropertyChanged(nameof(UseRangeSnapping));
            }
        }
    }

    public bool UseSliderSnap
    {
        get => _settings.UseSliderSnap;
        set
        {
            if (_settings.UseSliderSnap != value)
            {
                _settings.UseSliderSnap = value;
                RaisePropertyChanged(nameof(UseSliderSnap));
            }
        }
    }

    public EarTrumpetCommunitySettingsPageViewModel(AppSettings settings) : base(null)
    {
        _settings = settings;
        Title = Properties.Resources.CommunitySettingsPageText;
        Glyph = "\xE902";
    }
}