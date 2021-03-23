using EarTrumpet.DataModel.Storage;
using EarTrumpet.Extensibility.Shared;
using EarTrumpet.Extensions;
using Newtonsoft.Json;
using System.IO;

namespace EarTrumpet.Extensibility
{
    interface IAddonInternal
    {
        void Initialize();
        void InitializeInternal(AddonManifest manifest);
        bool IsInternal { get; set; }
    }

    public class EarTrumpetAddon : IAddonInternal
    {
        public string DisplayName { get; protected set; }
        public ISettingsBag Settings { get; private set; }
        public AddonManifest Manifest { get; private set; }

        bool IAddonInternal.IsInternal { get; set; }

        void IAddonInternal.Initialize()
        {
            var manifestPath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "AddonManifest.json");
            Manifest = JsonConvert.DeserializeObject<AddonManifest>(File.ReadAllText(manifestPath));
            Settings = StorageFactory.GetSettings(Manifest.Id);
        }

        void IAddonInternal.InitializeInternal(AddonManifest manifest)
        {
            Manifest = manifest;
            Settings = StorageFactory.GetSettings(Manifest.Id);
            ((IAddonInternal)this).IsInternal = true;
        }

        protected void LoadAddonResources()
        {
            ResourceLoader.Load(Manifest.Id, this.IsInternal());
        }
    }
}
