using EarTrumpet.Extensions;

namespace EarTrumpet.DataModel.Storage
{
    public class StorageFactory
    {
        private static ISettingsBag s_globalSettings;

        static StorageFactory()
        {
            s_globalSettings = App.Current.HasIdentity() ? (ISettingsBag)new WindowsStorageSettingsBag() : new RegistrySettingsBag();
        }

        public static ISettingsBag GetSettings(string nameSpace = null)
        {
            return (nameSpace == null) ? s_globalSettings :
                new NamespacedSettingsBag(nameSpace, s_globalSettings);
        }
    }
}
