using EarTrumpet.Extensions;
using EarTrumpet.Interop;
using EarTrumpet.Interop.MMDeviceAPI;
using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.WindowsAudio.Internal
{
    class Helpers
    {
        public static float[] ReadPeakValues(IAudioMeterInformation meter)
        {
            var ret = new float[2];
            try
            {
                uint chanCount = meter.GetMeteringChannelCount();
                if (chanCount > 0)
                {
                    var arrayPtr = Marshal.AllocHGlobal((int)chanCount * 4); // 4 bytes in float
                    if (meter.GetChannelsPeakValues(chanCount, arrayPtr) == HRESULT.S_OK)
                    {
                        var values = new float[chanCount];
                        Marshal.Copy(arrayPtr, values, 0, (int)chanCount);

                        if (chanCount == 1)
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
}
