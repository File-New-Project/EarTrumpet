using System;

namespace EarTrumpet
{
    enum Feature
    {
        DevicePopup,
    }

    class Features
    {
        public static bool IsEnabled(Feature feature)
        {
            switch (feature)
            {
                case Feature.DevicePopup:
#if DEBUG
                    return true;
#else
                    return false;
#endif
                default: throw new NotImplementedException();
            }
        }
    }
}
