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

        public string[] AdditionalPaths
        {
            get => StorageFactory.GetSettings().Get("Addons", new string[] { });
            set => StorageFactory.GetSettings().Set("Addons", value);
        }

        private AddonHost _host;

        public void Load()
        {
            _host = new AddonHost();
            var paths = _host.Initialize(AdditionalPaths);

            var ourPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToLower();
            foreach (var path in paths)
            {
                if (Path.GetDirectoryName(path).ToLower() == ourPath)
                {
                    BuiltIn.Add(new Addon(Path.GetFileName(path)));
                }
                else
                {
                    ThirdParty.Add(new Addon(path));
                }
            }
        }
    }
}
