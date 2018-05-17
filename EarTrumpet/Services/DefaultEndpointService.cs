using EarTrumpet.Interop;

namespace EarTrumpet.DataModel.Services
{
    public static class DefaultEndPointService
    {
        static IPolicyConfig s_PolicyConfigClient = null;

        public static void SetDefaultDevice(IAudioDevice device, ERole role = ERole.eMultimedia)
        {
            if (s_PolicyConfigClient == null)
            {
                s_PolicyConfigClient = (IPolicyConfig)new PolicyConfigClient();
            }

            s_PolicyConfigClient.SetDefaultEndpoint(device.Id, role);
        }
    }
}