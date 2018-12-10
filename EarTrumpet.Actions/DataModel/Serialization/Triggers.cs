using EarTrumpet.Interop.Helpers;
using EarTrumpet_Actions.DataModel.Enum;
using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Serialization
{
    [XmlInclude(typeof(EventTrigger))]
    [XmlInclude(typeof(HotkeyTrigger))]
    [XmlInclude(typeof(DeviceEventTrigger))]
    [XmlInclude(typeof(AppEventTrigger))]
    [XmlInclude(typeof(ProcessTrigger))]
    [XmlInclude(typeof(ContextMenuTrigger))]
    public abstract class BaseTrigger : Part { }

    public class AppEventTrigger : BaseTrigger, IPartWithDevice, IPartWithApp
    {
        public Device Device { get; set; }
        public App App { get; set; }
        public AudioAppEventKind Option { get; set; }
    }

    public class ContextMenuTrigger : BaseTrigger { }

    public class DeviceEventTrigger : BaseTrigger, IPartWithDevice
    {
        public Device Device { get; set; }
        public AudioDeviceEventKind Option { get; set; }
    }

    public class EventTrigger : BaseTrigger
    {
        public EarTrumpetEventKind Option { get; set; }
    }

    public class HotkeyTrigger : BaseTrigger
    {
        public HotkeyData Option { get; set; } = new HotkeyData();
    }

    public class ProcessTrigger : BaseTrigger, IPartWithText
    {
        public string Text { get; set; }
        public ProcessEventKind Option { get; set; }
    }
}
