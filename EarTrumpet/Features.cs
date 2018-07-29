using System;

namespace EarTrumpet
{
    enum Feature
    {
        Addons = 1,
    }

    class Features
    {
        public static bool IsEnabled(Feature feature)
        {
            switch (feature)
            {
                case Feature.Addons:
#if VSDEBUG
                    return true;
#else
                    return false;
#endif
                default: throw new NotImplementedException();
            }
        }
    }
}
