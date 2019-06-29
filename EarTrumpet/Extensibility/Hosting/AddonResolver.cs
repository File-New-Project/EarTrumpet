using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel;

namespace EarTrumpet.Extensibility.Hosting
{
    class AddonResolver
    {
        private readonly List<string> _addonDirectoryPaths = new List<string>();

        public AddonResolver()
        {
            // Search in addon directories when the framework can't otherwise resolve an assembly.
            AppDomain.CurrentDomain.AssemblyResolve += OnFinalAssemblyResolve;
        }

        public IEnumerable<DirectoryCatalog> Load(object target)
        {
            var catalogs = new List<DirectoryCatalog>();
            Trace.WriteLine($"AddonResolver Load");
            try
            {
                if (App.HasIdentity)
                {
                    catalogs.AddRange(Package.Current.Dependencies.
                        Where(pkg => pkg.IsOptional).
                        Where(pkg => pkg.PublisherDisplayName == Package.Current.PublisherDisplayName).
                        Select(pkg => SelectAddon(pkg.InstalledLocation.Path)).
                        Where(catalog => catalog != null));
                }
                else
                {
#if DEBUG
                    var rootAddonDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    catalogs.AddRange(Directory.GetDirectories(rootAddonDir, "PackageTemp*").
                        Select(path => SelectAddon(path)).
                        Where(catalog => catalog != null));
#endif
                }

                new CompositionContainer(new AggregateCatalog(catalogs)).ComposeParts(target);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"AddonResolver Load: {ex}");
            }
            return catalogs;
        }

        // Select an addon version that is equal to or lower than the current EarTrumpet version.
        // New addons are implicitly compatible.
        // If no lower-or-equal version is found, the addon is incompatible and won't be loaded.
        private DirectoryCatalog SelectAddon(string path)
        {
            try
            {
                Trace.WriteLine($"AddonResolver SelectAddon: {path}");
                var versionRoot = Path.Combine(path, "Versions");
                var versions = Directory.GetDirectories(versionRoot).Select(f => Path.GetFileName(f)).Select(f => Version.Parse(f)).OrderBy(v => v);
                foreach (var version in versions.Reverse())
                {
                    if (version <= App.PackageVersion)
                    {
                        var cat = new DirectoryCatalog(Path.Combine(versionRoot, version.ToString()), "EarTrumpet*.dll");
                        _addonDirectoryPaths.Add(cat.Path);
                        return cat;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"AddonResolver SelectAddon: {ex}");
            }
            Trace.WriteLine($"AddonResolver SelectAddon: Return without selection: {path}");
            return null;
        }

        private Assembly OnFinalAssemblyResolve(object sender, ResolveEventArgs args)
        {
            foreach (var path in _addonDirectoryPaths)
            {
                string assemblyPath = Path.Combine(path, new AssemblyName(args.Name).Name + ".dll");
                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
            }
            return null;
        }
    }
}
