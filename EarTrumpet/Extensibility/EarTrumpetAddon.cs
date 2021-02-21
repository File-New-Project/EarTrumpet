using EarTrumpet.DataModel.Storage;
using EarTrumpet.Extensibility.Shared;
using Newtonsoft.Json;
using System.IO;

namespace EarTrumpet.Extensibility
{
    interface IAddonInternal
    {
        void Initialize();
    }

    public class EarTrumpetAddon : IAddonInternal
    {
        public string DisplayName { get; protected set; }
        public ISettingsBag Settings { get; private set; }
        public AddonManifest Manifest { get; private set; }

        void IAddonInternal.Initialize()
        {
            var manifestPath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "AddonManifest.json");
            Manifest = JsonConvert.DeserializeObject<AddonManifest>(File.ReadAllText(manifestPath));
            Settings = StorageFactory.GetSettings(Manifest.Id);
        }

        protected void LoadAddonResources()
        {
            ResourceLoader.Load(Manifest.Id);
        }
    }
}
