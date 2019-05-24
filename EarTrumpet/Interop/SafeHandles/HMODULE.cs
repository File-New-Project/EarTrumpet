using Microsoft.Win32.SafeHandles;

namespace EarTrumpet.Interop.SafeHandles
{
    public class HMODULE : SafeHandleZeroOrMinusOneIsInvalid
    {
        private HMODULE() : base(ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return Kernel32.FreeLibrary(handle);
        }
    }
}