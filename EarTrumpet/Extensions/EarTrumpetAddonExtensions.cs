using EarTrumpet.Actions;
using EarTrumpet.Extensibility;

namespace EarTrumpet.Extensions
{
    public static class EarTrumpetAddonExtensions
    {
        public static bool IsInternal(this EarTrumpetAddon addon)
        {
            return addon is EarTrumpetActionsAddon;
        }
    }
}
