namespace EarTrumpet.DataModel.Storage
{
    public class StorageFactory
    {
        private static ISettingsBag s_globalSettings;

        static StorageFactory()
        {
            s_globalSettings = App.HasIdentity ? (ISettingsBag)new Internal.WindowsStorageSettingsBag() : new Internal.RegistrySettingsBag();
        }

        public static ISettingsBag GetSettings(string nameSpace = null)
        {
            return (nameSpace == null) ? s_globalSettings :
                new Internal.NamespacedSettingsBag(nameSpace, s_globalSettings);
        }
    }
}
