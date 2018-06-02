using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EarTrumpet.Interop
{
    class shlwapi
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = true)]
        internal static extern int SHLoadIndirectString(
            string pszSource, 
            StringBuilder pszOutBuf, 
            int cchOutBuf, 
            IntPtr ppvReserved);
    }
}
