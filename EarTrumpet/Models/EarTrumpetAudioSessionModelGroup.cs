using System.Collections.Generic;

namespace EarTrumpet.Models
{
    public class EarTrumpetAudioSessionModelGroup
    {
        public EarTrumpetAudioSessionModelGroup(IEnumerable<EarTrumpetAudioSessionModel> sessions)
        {
            Sessions = sessions;
        }
        public IEnumerable<EarTrumpetAudioSessionModel> Sessions { get; private set; }
    }
}
