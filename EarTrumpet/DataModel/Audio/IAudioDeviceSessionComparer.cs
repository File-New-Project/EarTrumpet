using System.Collections.Generic;

namespace EarTrumpet.DataModel.Audio
{
    public class IAudioDeviceSessionComparer : IEqualityComparer<IAudioDeviceSession>
    {
        public static readonly IAudioDeviceSessionComparer Instance = new IAudioDeviceSessionComparer();

        public bool Equals(IAudioDeviceSession x, IAudioDeviceSession y)
        {
            return x.AppId.Equals(y.AppId);
        }

        public int GetHashCode(IAudioDeviceSession obj)
        {
            return obj.AppId.GetHashCode();
        }
    }
}
