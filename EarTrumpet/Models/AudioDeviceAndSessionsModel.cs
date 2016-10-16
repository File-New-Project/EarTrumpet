using System.Collections.Generic;

namespace EarTrumpet.Models
{
    public class AudioDeviceAndSessionsModel
    {
        public string Id;
        public string DisplayName;
        public bool IsDefault;
        public bool IsMuted;
        public IEnumerable<AudioSessionModel> Sessions;
    }
}