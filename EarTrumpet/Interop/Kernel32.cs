using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EarTrumpet.Interop
{
    public class Kernel32
    {
        internal const int PACKAGE_INFORMATION_BASIC = 0x00000000;
        internal const int PACKAGE_FILTER_HEAD = 0x00000010;
        internal const int MAX_AUMID_LEN = 512;
        internal const int PACKAGE_FAMILY_NAME_MAX_LENGTH_INCL_Z = 65 * 2;
        internal const int PACKAGE_RELATIVE_APPLICATION_ID_MAX_LENGTH_INCL_Z = 65 * 2;
        internal const int PROCESS_QUERY_INFORMATION = 0x0400;
        internal const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        [Flags]
        public enum LoadLibraryFlags
        {
            LOAD_LIBRARY_AS_DATAFILE = 0x02,
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x20
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        public static extern IntPtr LoadLibraryEx(
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            IntPtr reserved,
            LoadLibraryFlags flags);

        [DllImport("kernel32.dll", PreserveSig = true)]
        public static extern int FreeLibrary(
            IntPtr moduleHandle);

        [DllImport("kernel32.dll", PreserveSig = true)]
        internal static extern IntPtr OpenProcess(
            uint dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId);

        [DllImport("kernel32.dll", PreserveSig = true)]
        internal static extern bool CloseHandle(
            IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern void GetApplicationUserModelId(
            IntPtr hProcess,
            ref int applicationUserModelIdLength,
            [MarshalAs(UnmanagedType.LPWStr)]StringBuilder applicationUserModelId);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern void ParseApplicationUserModelId(
            [MarshalAs(UnmanagedType.LPWStr)]string applicationUserModelId,
            ref int packageFamilyNameLength,
            StringBuilder packageFamilyName,
            ref int packageRelativeApplicationIdLength,
            StringBuilder packageRelativeApplicationId);

        [DllImport("kernel32.dll", EntryPoint = "FindPackagesByPackageFamily", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern int FindPackagesByPackageFamilyInitial(
            [MarshalAs(UnmanagedType.LPWStr)]string packageFamilyName,
            int packageFilters,
            ref int count,
            IntPtr packageFullNames,
            ref int bufferLength,
            IntPtr buffer,
            IntPtr packageProperties);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern int FindPackagesByPackageFamily(
            [MarshalAs(UnmanagedType.LPWStr)]string packageFamilyName,
            int packageFilters,
            ref int count,
            IntPtr[] packageFullNames,
            ref int bufferLength,
            IntPtr buffer,
            IntPtr packageProperties);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern int OpenPackageInfoByFullName(
            [MarshalAs(UnmanagedType.LPWStr)]string packageFullName,
            int reserved,
            out IntPtr packageInfoReference);

        [DllImport("kernel32.dll", PreserveSig = true)]
        internal static extern int GetPackageApplicationIds(
            IntPtr packageInfoReference,
            ref int bufferLength,
            IntPtr buffer,
            out int count);

        [DllImport("kernel32.dll", PreserveSig = true)]
        internal static extern int ClosePackageInfo(
            IntPtr packageInfoReference);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern int QueryFullProcessImageName(
            IntPtr hProcess,
            uint dwFlags,
            [MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpExeName,
            ref uint lpdwSize);
    }
}
