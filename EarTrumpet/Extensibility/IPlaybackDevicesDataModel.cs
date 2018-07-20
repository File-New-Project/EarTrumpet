using EarTrumpet.DataModel;

namespace EarTrumpet.Extensibility
{
    public interface IPlaybackDevicesDataModel
    {
        void InitializeDataModel(IAudioDeviceManager devices);
    }
}
