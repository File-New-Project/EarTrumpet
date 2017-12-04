using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Com
{
    [Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMNotificationClient
    {
        void OnDeviceStateChanged(string pwstrDeviceId, DEVICE_STATE dwNewState);
        void OnDeviceAdded(string pwstrDeviceId);
        void OnDeviceRemoved(string pwstrDeviceId);
        void OnDefaultDeviceChanged(EDataFlow flow, ERole role, string pwstrDefaultDeviceId);
        void OnPropertyValueChanged(string pwstrDeviceId, PROPERTYKEY key);
    }
}