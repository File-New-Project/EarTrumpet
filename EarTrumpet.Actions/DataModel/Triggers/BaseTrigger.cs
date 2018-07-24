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
    public abstract class BaseTrigger : Part
    {
        public event Action Triggered;

        public abstract void Close();

        protected void OnTriggered()
        {
            Trace.WriteLine("Triggered!!!!!");
            Triggered?.Invoke();
        }
    }
}
