using System.Collections.Generic;

namespace EarTrumpet.Models
{
    public class EarTrumpetAudioSessionModelGroup
    {
        public EarTrumpetAudioSessionModelGroup(IEnumerable<AudioSessionModel> sessions)
        {
            Sessions = sessions;
        }
        public IEnumerable<AudioSessionModel> Sessions { get; private set; }
    }
}
