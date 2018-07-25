using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    [XmlInclude(typeof(EventTrigger))]
    [XmlInclude(typeof(HotkeyTrigger))]
    [XmlInclude(typeof(AudioDeviceEventTrigger))]
    [XmlInclude(typeof(AudioDeviceSessionEventTrigger))]
    [XmlInclude(typeof(ProcessTrigger))]
    public abstract class BaseTrigger : PartWithOptions
    {
        public event Action Triggered;

        public virtual void Close()
        {

        }

        protected void RaiseTriggered()
        {
            Triggered?.Invoke();
        }
    }
}
