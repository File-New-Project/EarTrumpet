using EarTrumpet.DataModel.Storage;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace EarTrumpet.Extensibility.Hosting
{
    class AddonManager
    {
        public static AddonManager Current { get; } = new AddonManager();

        public List<Addon> BuiltIn { get; } = new List<Addon>();
        public List<Addon> ThirdParty { get; } = new List<Addon>();

        public string[] UserDefinedAddons
        {
            get => StorageFactory.GetSettings().Get("Addons", new string[] { });
            set => StorageFactory.GetSettings().Set("Addons", value);
        }

        private AddonHost _host;

        public void Load()
        {
            if (Features.IsEnabled(Feature.Addons))
            {
                _host = new AddonHost();
                var entries = _host.Initialize(UserDefinedAddons);

                var ourPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToLower();
                foreach (var entry in entries)
                {
                    if (entry.IsThirdParty)
                    {
                        ThirdParty.Add(new Addon(entry.Catalog));
                    }
                    else
                    {
                        BuiltIn.Add(new Addon(entry.Catalog));
                    }
                }
            }
        }
    }
}
