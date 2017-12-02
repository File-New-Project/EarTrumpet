using System.ComponentModel;

namespace EarTrumpet.DataModel
{
    public interface IAudioDevice : IStreamWithVolumeControl
    {


        AudioDeviceSessionCollection Sessions { get; }
    }
}