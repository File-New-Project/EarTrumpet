using EarTrumpet.DataModel.Com;
using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.DataModel
{
    static class DefaultEndPoint
    {
        [Guid("CA286FC3-91FD-42C3-8E9B-CAAFA66242E3")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPolicyConfig_TH1
        {
            void Unused1();
            void Unused2();
            void Unused3();
            void Unused4();
            void Unused5();
            void Unused6();
            void Unused7();
            void Unused8();
            void Unused9();
            void Unused10();
            void SetDefaultEndpoint(string wszDeviceId, uint eRole);
        }

        [Guid("6BE54BE8-A068-4875-A49D-0C2966473B11")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPolicyConfig_TH2
        {
            void Unused1();
            void Unused2();
            void Unused3();
            void Unused4();
            void Unused5();
            void Unused6();
            void Unused7();
            void Unused8();
            void Unused9();
            void Unused10();
            void SetDefaultEndpoint(string wszDeviceId, uint eRole);
        }

        [Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPolicyConfig_RS1
        {
            void Unused1();
            void Unused2();
            void Unused3();
            void Unused4();
            void Unused5();
            void Unused6();
            void Unused7();
            void Unused8();
            void Unused9();
            void Unused10();
            void SetDefaultEndpoint(string wszDeviceId, uint eRole);
        }

        [ComImport]
        [Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
        public class PolicyConfigClient { }

        public static void SetDefaultDevice(IAudioDevice device)
        {
            var policyClient = new PolicyConfigClient();

            var policy_th1 = policyClient as IPolicyConfig_TH1;
            if (policy_th1 != null)
            {
                policy_th1.SetDefaultEndpoint(device.Id, (uint)ERole.eMultimedia);
                return;
            }

            var policy_th2 = policyClient as IPolicyConfig_TH2;
            if (policy_th2 != null)
            {
                policy_th2.SetDefaultEndpoint(device.Id, (uint)ERole.eMultimedia);
                return;
            }

            var policy_rs1 = policyClient as IPolicyConfig_RS1;
            if (policy_rs1 != null)
            {
                policy_rs1.SetDefaultEndpoint(device.Id, (uint)ERole.eMultimedia);
                return;
            }

            throw new Exception("IPolicyClient is not available.");
        }
    }
}