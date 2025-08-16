namespace EarTrumpet.UI.ViewModels;

public class EarTrumpetCommunitySettingsPageViewModel : SettingsPageViewModel
{
    private readonly AppSettings _settings;
    public bool UseLogarithmicVolume
    {
        get => _settings.UseLogarithmicVolume;
        set => _settings.UseLogarithmicVolume = value;
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

    public EarTrumpetCommunitySettingsPageViewModel(AppSettings settings) : base(null)
    {
        _settings = settings;
        Title = Properties.Resources.CommunitySettingsPageText;
        Glyph = "\xE902";
    }
}