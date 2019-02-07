using EarTrumpet.Extensions;
using EarTrumpet.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

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

        [ImportMany(typeof(IAddonAppContextMenu))]
        private List<IAddonAppContextMenu> _appContextMenuItems { get; set; }

        [ImportMany(typeof(IAddonDeviceContent))]
        private List<IAddonDeviceContent> _deviceContentItems { get; set; }

        [ImportMany(typeof(IAddonDeviceContextMenu))]
        private List<IAddonDeviceContextMenu> _deviceContextMenuItems { get; set; }

        [ImportMany(typeof(IAddonSettingsPage))]
        private List<IAddonSettingsPage> _settingsPages { get; set; }

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

        public IEnumerable<Addon> Initialize()
        {
            var catalogs = new List<DirectoryCatalog>();
            Trace.WriteLine($"AddonHost Initialize");
            try
            {
                if (App.Current.HasIdentity())
                {
                    foreach (var package in Windows.ApplicationModel.Package.Current.Dependencies)
                    {
                        if (package.IsOptional)
                        {
                            Trace.WriteLine($"AddonHost: Loading from: {package.InstalledLocation.Path}");
                            catalogs.Add(new DirectoryCatalog(package.InstalledLocation.Path, "*.dll"));
                        }
                    }
                }
                else
                {
#if DEBUG
                    try
                    {
                        var rootAddonDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "addons");
                        if (Directory.Exists(rootAddonDir))
                        {
                            foreach (var directoryPath in Directory.GetDirectories(rootAddonDir))
                            {
                                catalogs.Add(new DirectoryCatalog(directoryPath, "EarTrumpet-*.dll"));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
#endif
                }

                var container = new CompositionContainer(new AggregateCatalog(catalogs));
                container.ComposeParts(this);

                var ret = catalogs.Select(c => new Addon(c, FindInfo(c))).ToList();

                foreach (var addon in ret.ToArray())
                {
                    if (!addon.IsValid)
                    {
                        ret.Remove(addon);
                    }
                }

                foreach (var addon in ret)
                {
                    if (!addon.IsCompatible)
                    {
                        RemoveAddon(_contextMenuItems, addon);
                        RemoveAddon(_appContextMenuItems, addon);
                        RemoveAddon(_appContentItems, addon);
                        RemoveAddon(_deviceContextMenuItems, addon);
                        RemoveAddon(_deviceContentItems, addon);
                        RemoveAddon(_settingsPages, addon);
                    }
                }
                TrayViewModel.AddonItems = _contextMenuItems.ToArray();
                FocusedAppItemViewModel.AddonContextMenuItems = _appContextMenuItems.ToArray();
                FocusedAppItemViewModel.AddonContentItems = _appContentItems.ToArray();
                FocusedDeviceViewModel.AddonContextMenuItems = _deviceContextMenuItems.ToArray();
                FocusedDeviceViewModel.AddonContentItems = _deviceContentItems.ToArray();
                SettingsViewModel.AddonItems = _settingsPages.ToArray();

                _appLifecycle.ToList().ForEachNoThrow(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Startup));
                _appLifecycle.ToList().ForEachNoThrow(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Startup2));
                App.Current.Exit += (_, __) => _appLifecycle.ToList().ForEachNoThrow(x => x.OnApplicationLifecycleEvent(ApplicationLifecycleEvent.Shutdown));

                return ret;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"AddonHost Initialize: {ex}");
            }
            return null;
        }

        private void RemoveAddon<T>(List<T> list, Addon addon)
        {
            foreach (var item in list.ToArray())
            {
                if (addon.IsAssembly(item.GetType().Assembly))
                {
                    list.Remove(item);
                }
            }
        }
    }
}