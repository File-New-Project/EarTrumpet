using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop.MMDeviceAPI
{
    [ComImport]
    [Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
    class PolicyConfigClient { }

    class AutoPolicyConfigClient
    {
        PolicyConfigClient _policyClient = new PolicyConfigClient();

        public void SetDefaultEndpoint(string deviceId, ERole role = ERole.eMultimedia)
        {
            var policy_rs1 = _policyClient as IPolicyConfig_RS1;
            if (policy_rs1 != null)
            {
                policy_rs1.SetDefaultEndpoint(deviceId, (uint)role);
                return;
            }

            var policy_th1 = _policyClient as IPolicyConfig_TH1;
            if (policy_th1 != null)
            {
                policy_th1.SetDefaultEndpoint(deviceId, (uint)role);
                return;
            }

            var policy_th2 = _policyClient as IPolicyConfig_TH2;
            if (policy_th2 != null)
            {
                policy_th2.SetDefaultEndpoint(deviceId, (uint)role);
                return;
            }

            throw new Exception("IPolicyClient is not available.");
        }
    }
}
