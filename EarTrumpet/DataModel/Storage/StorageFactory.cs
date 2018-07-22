namespace EarTrumpet.DataModel.Storage
{
    public class StorageFactory
    {
        private static GlobalSettingsBag s_globalSettings = new GlobalSettingsBag();

        public static ISettingsBag GetSettings(string nameSpace=null)
        {
            if (nameSpace == null)
            {
                return s_globalSettings;
            }
            else
            {
                return new NamespacedSettingsBag(nameSpace, s_globalSettings);
            }
        }
    }
}
