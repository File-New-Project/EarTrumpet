using EarTrumpet.DataModel.Com;
using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel
{
    static class DefaultEndPoint
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
            void SetDefaultEndpoint(string wszDeviceId, uint eRole);
        }

        [ComImport]
        [Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
        public class PolicyConfigClient { }

        public static void SetDefaultDevice(IAudioDevice device, ERole role = ERole.eMultimedia)
        {
            var policyClient = new PolicyConfigClient();
            (policyClient as IPolicyConfig).SetDefaultEndpoint(device.Id, (uint)role);
        }
    }
}