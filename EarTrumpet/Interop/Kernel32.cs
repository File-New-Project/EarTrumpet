using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EarTrumpet.Interop
{
    class Kernel32
    {
        internal const int SIZEOF_WCHAR = 2;
        internal const int PACKAGE_INFORMATION_BASIC = 0x00000000;
        internal const int PACKAGE_FILTER_HEAD = 0x00000010;
        internal const int MAX_AUMID_LEN = 512;
        internal const int PACKAGE_FAMILY_NAME_MAX_LENGTH_INCL_Z = 65 * SIZEOF_WCHAR;
        internal const int PACKAGE_RELATIVE_APPLICATION_ID_MAX_LENGTH_INCL_Z = 65 * SIZEOF_WCHAR;

        [Flags]
        internal enum ProcessFlags : uint
        {
            PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
            SYNCHRONIZE = 0x00100000,
        }

        [Flags]
        internal enum LoadLibraryFlags : int
        {
            LOAD_LIBRARY_AS_DATAFILE = 0x02,
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x20
        }

        [Flags]
        internal enum IMAGE_FILE_MACHINE : int
        {
            // ...
            IMAGE_FILE_MACHINE_I386 = 0x014c,
            IMAGE_FILE_MACHINE_AMD64 = 0x8664,
            IMAGE_FILE_MACHINE_ARM64 = 0xAA64
            // ...
        }

        internal const int WAIT_OBJECT_0 = 0x00000000;
        internal const int WAIT_FAILED = unchecked((int)0xFFFFFFFF);
        internal const int WAIT_TIMEOUT = 0x00000102;
        internal const int WAIT_ABANDONED = 0x00000080;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern IntPtr LoadLibraryEx(
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            IntPtr reserved,
            LoadLibraryFlags flags);

        [DllImport("kernel32.dll", PreserveSig = true)]
        internal static extern int FreeLibrary(
            IntPtr moduleHandle);

        [DllImport("kernel32.dll", PreserveSig = true)]
        internal static extern IntPtr OpenProcess(
            ProcessFlags dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId);

        [DllImport("kernel32.dll", PreserveSig = true)]
        internal static extern bool CloseHandle(
            IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern int GetApplicationUserModelId(
            IntPtr hProcess,
            ref int applicationUserModelIdLength,
            [MarshalAs(UnmanagedType.LPWStr)]StringBuilder applicationUserModelId);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        internal static extern int ParseApplicationUserModelId(
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

        [DllImport("kernel32.dll", PreserveSig = true)]
        internal static extern int WaitForMultipleObjects(
            int nCount,
            IntPtr[] lpHandles,
            [MarshalAs(UnmanagedType.Bool)]bool bWaitAll,
            int dwMilliseconds);

        [DllImport("kernel32.dll", PreserveSig = true)]
        internal static extern int WaitForSingleObject(
            IntPtr lpHandle, 
            int dwMilliseconds);

        [DllImport("kernel32.dll", PreserveSig = true)]
        [return:MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process2(
            IntPtr hProcess,
            out IMAGE_FILE_MACHINE pProcessMachine,
            out IMAGE_FILE_MACHINE pNativeMachine);

        public static IntPtr RT_ICON = new IntPtr(3);
        public static IntPtr RT_GROUP_ICON = new IntPtr(14);

        [DllImport("kernel32.dll", PreserveSig = true)]
        public static extern IntPtr FindResourceW(
            IntPtr hModule,
            IntPtr lpName,
            IntPtr lpType);

        [DllImport("kernel32.dll", PreserveSig = true)]
        public static extern IntPtr LoadResource(
            IntPtr hModule, 
            IntPtr hResInfo);

        [DllImport("kernel32.dll", PreserveSig = true)]
        public static extern IntPtr LockResource(
            IntPtr hResData);

        [DllImport("kernel32.dll", PreserveSig = true)]
        public static extern int SizeofResource(
            IntPtr hModule,
            IntPtr hResInfo);
    }
}
