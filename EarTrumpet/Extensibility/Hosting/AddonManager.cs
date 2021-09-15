using EarTrumpet.Actions;
using EarTrumpet.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EarTrumpet.Extensibility.Hosting
{
    public class AddonManager
    {
        public static AddonHost Host { get; } = new AddonHost();

        private static readonly AddonResolver s_resolver = new AddonResolver();
        private static readonly List<string> s_loadedAddonIds = new List<string>();
        private static bool s_shouldLoadInternalAddons = false;

        public static void Load(bool shouldLoadInternalAddons = false)
        {
            s_shouldLoadInternalAddons = shouldLoadInternalAddons;

            Host.Addons = new List<EarTrumpetAddon>();
            var loadedCatalogs = s_resolver.Load(Host);
            {
                foreach (var addon in Host.Addons.ToArray())
                {
                    try
                    {
                        ((IAddonInternal)addon).Initialize();
                        s_loadedAddonIds.Add(addon.Manifest.Id);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"AddonManager Load: Failed to initialize addon: {addon.GetType().Assembly.FullName} {ex}");
                        Host.Addons.Remove(addon);
                    }
                }
            }

            if (s_shouldLoadInternalAddons)
            {
                LoadInternalAddons();
            }

            Host.Events = Host.Addons.Where(a => a is IEarTrumpetAddonEvents).Select(a => (IEarTrumpetAddonEvents)a).ToList();
            Host.TrayContextMenuItems = Host.Addons.Where(a => a is IEarTrumpetAddonNotificationAreaContextMenu).Select(a => (IEarTrumpetAddonNotificationAreaContextMenu)a).ToList();
            Host.AppContentItems = Host.Addons.Where(a => a is IEarTrumpetAddonAppContent).Select(a => (IEarTrumpetAddonAppContent)a).ToList();
            Host.DeviceContentItems = Host.Addons.Where(a => a is IEarTrumpetAddonDeviceContent).Select(a => (IEarTrumpetAddonDeviceContent)a).ToList();
            Host.SettingsItems = Host.Addons.Where(a => a is IEarTrumpetAddonSettingsPage).Select(a => (IEarTrumpetAddonSettingsPage)a).ToList();

            Host.Events.ForEachNoThrow(x => x.OnAddonEvent(AddonEventKind.InitializeAddon));
            Host.Events.ForEachNoThrow(x => x.OnAddonEvent(AddonEventKind.AddonsInitialized));
        }

        private static void LoadInternalAddons()
        {
            var actions = new EarTrumpetActionsAddon();
            Host.Addons.Add(actions);
            ((IAddonInternal)actions).InitializeInternal(new AddonManifest { Id = "EarTrumpet.Actions" });
        }

        public static void Shutdown()
        {
            Trace.WriteLine($"AddonManager Shutdown");
            Host.Events.ForEachNoThrow(x => x.OnAddonEvent(AddonEventKind.AppShuttingDown));
        }

        public static string GetDiagnosticInfo() => string.Join(" ", s_loadedAddonIds);
    }
}