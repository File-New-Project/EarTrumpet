using System;
using System.Runtime.InteropServices;

namespace EarTrumpet.Interop
{
    class Ntdll
    {
        public enum SYSTEM_INFORMATION_CLASS
        {
            /* ... */
            SystemProcessInformation = 0x0005,
            /* ... */
        }

        #if X86
        [StructLayout(LayoutKind.Explicit)]
        public struct SYSTEM_PROCESS_INFORMATION
        {
            [FieldOffset(0)]
            public int NextEntryOffset;
            /* ... */
            [FieldOffset(56)]
            public UNICODE_STRING ImageName;
            /* ... */
            [FieldOffset(68)]
            public int UniqueProcessId;
            /* ... */
        }
        #else
        #error Platform not supported.
        #endif

        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct LARGE_INTEGER
        {
            [FieldOffset(0)]
            public Int64 QuadPart;
            [FieldOffset(0)]
            public UInt32 LowPart;
            [FieldOffset(4)]
            public Int32 HighPart;
        }

        [StructLayout(LayoutKind.Sequential, Size = 8)]
        public struct UNICODE_STRING
        {
            public short Length;
            public short MaximumLength;
            public IntPtr Buffer;
        }

        public enum NTSTATUS : uint
        {
            SUCCESS = 0x0,
            STATUS_INFO_LENGTH_MISMATCH = 0xC0000004
        }

        [DllImport("ntdll.dll", PreserveSig = true, EntryPoint = "NtQuerySystemInformation")]
        public static extern NTSTATUS NtQuerySystemInformationInitial(
            SYSTEM_INFORMATION_CLASS infoClass,
            IntPtr info,
            int size,
            out int length);

        [DllImport("ntdll.dll", PreserveSig = true)]
        public static extern NTSTATUS NtQuerySystemInformation(
            SYSTEM_INFORMATION_CLASS InfoClass,
            IntPtr info,
            int size,
            IntPtr length);
    }
}
