using EarTrumpet.DataModel.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        internal Addon FindAddonForObject(object addonObject)
        {
            var asm = addonObject.GetType().Assembly;
            var ret = BuiltIn.FirstOrDefault(a => a.IsAssembly(asm));
            if (ret == null)
            {
                ret = ThirdParty.FirstOrDefault(a => a.IsAssembly(asm));
            }
            return ret;
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
                        ThirdParty.Add(new Addon(entry.Catalog, entry.Info));
                    }
                    else
                    {
                        BuiltIn.Add(new Addon(entry.Catalog, entry.Info));
                    }
                }
            }
        }
    }
}
