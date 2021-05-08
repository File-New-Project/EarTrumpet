using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

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
                var rootAddonDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Addons");
                if (Directory.Exists(rootAddonDir))
                {
                    catalogs.AddRange(Directory.GetDirectories(rootAddonDir).
                        Select(path => SelectAddon(path)).
                        Where(catalog => catalog != null));

#if DEBUG
                    catalogs.AddRange(Directory.GetDirectories(rootAddonDir)
                        .Select(path => SelectDevAddon(path))
                        .Where(catalog => catalog != null));
#endif
                    new CompositionContainer(new AggregateCatalog(catalogs)).ComposeParts(target);
                }
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
                var versions = Directory.GetDirectories(path).Select(f => Path.GetFileName(f)).Select(f => Version.Parse(f)).OrderBy(v => v);
                foreach (var version in versions.Reverse())
                {
                    if (version <= App.PackageVersion)
                    {
                        var cat = new DirectoryCatalog(Path.Combine(path, version.ToString()), "EarTrumpet*.dll");
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


#if DEBUG
        // Discover addons in the form of AddonName\AddonName.dll.
        private DirectoryCatalog SelectDevAddon(string path)
        {
            try
            {
                Trace.WriteLine($"AddonResolver SelectDevAddon: Discovering from {path}");
                var cat = new DirectoryCatalog(path, $"{new DirectoryInfo(path).Name}.dll");
                if (cat.LoadedFiles.Count == 0)
                {
                    Trace.WriteLine("AddonResolver SelectDevAddon: ## WARNING ##: No files found in addon package");
                }
                else
                {
                    foreach (var file in cat.LoadedFiles)
                    {
                        Trace.WriteLine($"AddonResolver SelectDevAddon Loading: {file}");
                    }
                }
                _addonDirectoryPaths.Add(cat.Path);
                return cat;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"AddonResolver SelectDevAddon: {ex}");
            }
            Trace.WriteLine($"AddonResolver SelectDevAddon: Return without selection: {path}");
            return null;
        }
#endif

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
