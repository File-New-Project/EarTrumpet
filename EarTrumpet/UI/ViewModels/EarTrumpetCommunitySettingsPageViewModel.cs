namespace EarTrumpet.UI.ViewModels;

public class EarTrumpetCommunitySettingsPageViewModel : SettingsPageViewModel
{
    private readonly AppSettings _settings;
    public bool UseLogarithmicVolume
    {
        get => _settings.UseLogarithmicVolume;
        set => _settings.UseLogarithmicVolume = value;
    }

    public double LogarithmicVolumeMindB
    {
        get => _settings.LogarithmicVolumeMindB;
        set
        {
            if (_settings.LogarithmicVolumeMindB != (float)value)
            {
                _settings.LogarithmicVolumeMindB = (float)value;
                RaisePropertyChanged(nameof(LogarithmicVolumeMindB));
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