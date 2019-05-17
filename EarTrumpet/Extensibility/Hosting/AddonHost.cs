using EarTrumpet.Extensions;
using EarTrumpet.UI.ViewModels;
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
    class AddonHost
    {
        [ImportMany(typeof(IAddonLifecycle))]
        private List<IAddonLifecycle> _appLifecycle { get; set; }

        [ImportMany(typeof(IAddonContextMenu))]
        private List<IAddonContextMenu> _contextMenuItems { get; set; }

        [ImportMany(typeof(IAddonAppContent))]
        private List<IAddonAppContent> _appContentItems { get; set; }

        [ImportMany(typeof(IAddonDeviceContent))]
        private List<IAddonDeviceContent> _deviceContentItems { get; set; }

        [ImportMany(typeof(IAddonSettingsPage))]
        private List<IAddonSettingsPage> _settingsPages { get; set; }

        private readonly List<string> _addonDirectoryPaths = new List<string>();

        private AddonInfo FindInfo(DirectoryCatalog catalog)
        {
            foreach (var file in catalog.LoadedFiles)
            {
                foreach (var a in _appLifecycle)
                {
                    var asmLocation = a.GetType().Assembly.Location;
                    if (asmLocation.ToLower() == file.ToLower())
                    {
                        return a.Info;
                    }
                }
            }
            return null;
        }

        // Select an addon version that is equal to or lower than the current EarTrumpet version.
        // New addons are implicitly compatible.
        // If no lower-or-equal version is found, the addon is incompatible and won't be loaded.
        private DirectoryCatalog SelectAddon(string path)
        {
            try
            {
                Trace.WriteLine($"AddonHost: SelectAddon: {path}");
                var versionRoot = Path.Combine(path, "Versions");
                var versions = Directory.GetDirectories(versionRoot).Select(f => Path.GetFileName(f)).Select(f => Version.Parse(f)).OrderBy(v => v);
                var earTrumpetVersion = ((App)App.Current).GetVersion();
                foreach (var version in versions.Reverse())
                {
                    if (version <= earTrumpetVersion)
                    {
                        var cat = new DirectoryCatalog(Path.Combine(versionRoot, version.ToString()), "EarTrumpet*.dll");
                        _addonDirectoryPaths.Add(cat.Path);
                        return cat;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            Trace.WriteLine($"AddonHost: Return without selection: {path}");
            return null;
        }

        public IEnumerable<Addon> Initialize()
        {
            // Search in addon directories when the framework can't otherwise resolve an assembly.
            AppDomain.CurrentDomain.AssemblyResolve += OnFinalAssemblyResolve;

            var catalogs = new List<DirectoryCatalog>();
            Trace.WriteLine($"AddonHost Initialize");
            try
            {
                if (App.Current.HasIdentity())
                {
                    catalogs.AddRange(Windows.ApplicationModel.Package.Current.Dependencies.
                        Where(p => p.IsOptional).
                        Where(p => p.PublisherDisplayName == Package.Current.PublisherDisplayName).
                        Select(p => SelectAddon(p.InstalledLocation.Path)).
                        Where(a => a != null));
                }
                else
                {
#if DEBUG
                    var rootAddonDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    catalogs.AddRange(Directory.GetDirectories(rootAddonDir, "PackageTemp*").
                        Select(path => SelectAddon(path)).
                        Where(addon => addon != null));
#endif
                }

                new CompositionContainer(new AggregateCatalog(catalogs)).ComposeParts(this);

                App.AddonTrayContextMenuItems = _contextMenuItems.ToArray();
                FocusedAppItemViewModel.AddonContentItems = _appContentItems.ToArray();
                FocusedDeviceViewModel.AddonContentItems = _deviceContentItems.ToArray();
                SettingsViewModel.AddonItems = _settingsPages.ToArray();

                _appLifecycle.ToList().ForEachNoThrow(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Startup));
                _appLifecycle.ToList().ForEachNoThrow(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Startup2));

                return catalogs.Select(c => new Addon(c, FindInfo(c))).ToList().Where(a => a.IsValid);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"AddonHost Initialize: {ex}");
            }
            return null;
        }

        public void Shutdown()
        {
            _appLifecycle.ToList().ForEachNoThrow(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Shutdown));
        }

        private Assembly OnFinalAssemblyResolve(object sender, ResolveEventArgs args)
        {
            foreach(var path in _addonDirectoryPaths)
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