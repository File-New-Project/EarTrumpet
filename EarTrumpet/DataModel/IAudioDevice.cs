namespace EarTrumpet.DataModel
{
    public interface IAudioDevice : IStreamWithVolumeControl
    {
        IAudioDeviceSessionCollection Sessions { get; }
    }
}