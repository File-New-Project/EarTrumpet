using EarTrumpet.DataModel.Com;
using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel.Services
{
    public static class DefaultEndPointService
    {
        static class Interop
        {
            [Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IPolicyConfig
            {
                void __incomplete__1();
                void __incomplete__2();
                void __incomplete__3();
                void __incomplete__4();
                void __incomplete__5();
                void __incomplete__6();
                void __incomplete__7();
                void __incomplete__8();
                void __incomplete__9();
                void __incomplete__10();
                void SetDefaultEndpoint(string wszDeviceId, ERole eRole);
            }

            [ComImport]
            [Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
            public class PolicyConfigClient { }
        }

        static Interop.IPolicyConfig s_PolicyConfigClient = null;

        public static void SetDefaultDevice(IAudioDevice device, ERole role = ERole.eMultimedia)
        {
            if (s_PolicyConfigClient == null)
            {
                s_PolicyConfigClient = (Interop.IPolicyConfig)new Interop.PolicyConfigClient();
            }

            s_PolicyConfigClient.SetDefaultEndpoint(device.Id, role);
        }
    }
}