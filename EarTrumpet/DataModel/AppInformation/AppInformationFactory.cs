using EarTrumpet.Interop.Helpers;

namespace EarTrumpet.DataModel.AppInformation
{
    public class AppInformationFactory
    {
        public static IAppInfo CreateForProcess(int processId, bool trackProcess = false)
        {
            if (processId == 0)
            {
                return new Internal.SystemSoundsAppInfo();
            }

            if (User32Helper.IsImmersiveProcess(processId))
            {
                return new Internal.ModernAppInfo(processId, trackProcess);
            }
            else
            {
                return new Internal.DesktopAppInfo(processId, trackProcess);
            }
        }
    }
}
