

namespace EarTrumpet.UI.ViewModels;

public class EarTrumpetMouseSettingsPageViewModel : SettingsPageViewModel
{
    public bool UseScrollWheelInTray
    {
        get => _settings.UseScrollWheelInTray;
        set => _settings.UseScrollWheelInTray = value;
    }

    public bool UseGlobalMouseWheelHook
    {
        get => _settings.UseGlobalMouseWheelHook;
        set => _settings.UseGlobalMouseWheelHook = value;
    }

    private readonly AppSettings _settings;

    public EarTrumpetMouseSettingsPageViewModel(AppSettings settings) : base(null)
    {
        _settings = settings;
        Title = Properties.Resources.MouseSettingsPageText;
        Glyph = "\xE962";
    }
}