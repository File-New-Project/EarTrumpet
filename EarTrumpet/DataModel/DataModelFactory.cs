using EarTrumpet.DataModel.Internal;
using System.Windows.Threading;

namespace EarTrumpet.DataModel
{
    class DataModelFactory
    {
        public static IAudioDeviceManager CreateAudioDeviceManager()
        {
            return new AudioDeviceManager(Dispatcher.CurrentDispatcher);
        }
    }
}
