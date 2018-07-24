using EarTrumpet.Extensibility;
using System.ComponentModel.Composition;

namespace EarTrumpet_Actions
{
    [Export(typeof(IAddonLifecycle))]
    public class Addon : IAddonLifecycle
    {
        public void OnApplicationLifecycleEvent(ApplicationLifecycleEvent evt)
        {
            ActionsManager.Instance.OnEvent(evt);
        }
    }
}