using EarTrumpet.UI.ViewModels;

namespace EarTrumpet.Extensibility
{
    public interface IAddonSettingsPage
    {
        SettingsCategoryViewModel Get(AddonInfo addon);
    }
}
