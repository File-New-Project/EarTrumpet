using EarTrumpet.Interop.MMDeviceAPI;

namespace EarTrumpet.DataModel.Internal
{
    internal interface IAudioDeviceInternal
    {
        void DevicePropertiesChanged(IMMDevice device);
    }
}
