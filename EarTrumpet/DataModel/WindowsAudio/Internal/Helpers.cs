using System;
using EarTrumpet.Extensions;
using Windows.Win32.Media.Audio.Endpoints;

namespace EarTrumpet.DataModel.WindowsAudio.Internal;

internal class Helpers
{
    public static float[] ReadPeakValues(IAudioMeterInformation meter)
    {
        var ret = new float[2];
        try
        {
            meter.GetMeteringChannelCount(out var channelCount);
            if (channelCount > 0)
            {
                var values = new float[(int)channelCount];
                if (meter.GetChannelsPeakValues(channelCount, values) == HRESULT.S_OK)
                {
                    if (channelCount == 1)
                    {
                        ret[0] = values[0];
                        ret[1] = values[0];
                    }
                    else
                    {
                        ret[0] = values[0];
                        ret[1] = values[1];
                    }
                }
            }
        }
        catch (Exception ex) when (ex.Is(HRESULT.AUDCLNT_E_DEVICE_INVALIDATED))
        {
            // Expected in some cases.
        }
        return ret;
    }
}
