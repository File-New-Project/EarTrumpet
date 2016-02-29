using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.Services
{
    public sealed class ProcessTitleProviderFactoryService
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr GetWindow(IntPtr hwnd, GetWindowType wFlag);

        enum GetWindowType : int
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6,

        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public static ProcessTitleProvider CreateProvider()
        {
            var res = new Dictionary<uint, string>();
            EnumWindows((handle, lParam) =>
            {
                var hr = GetWindow(handle, GetWindowType.GW_OWNER);
                if (hr == (System.IntPtr)0 && IsWindowVisible(handle))
                {
                    uint processId;
                    GetWindowThreadProcessId(handle, out processId);
                    if (!res.ContainsKey(processId))
                    {
                        const int MAX_TITLE_LEN = 512;
                        var sb = new System.Text.StringBuilder(MAX_TITLE_LEN);
                        if (GetWindowText(handle, sb, MAX_TITLE_LEN - 1) > 0)
                        {
                            res.Add(processId, sb.ToString());
                        }
                    }
                }
                return true;
            }, System.IntPtr.Zero);

            return new ProcessTitleProvider(res);
        }
    }

    public sealed class ProcessTitleProvider
    {
        public ProcessTitleProvider(Dictionary<uint, string> processTitles)
        {
            this._processTitles = processTitles;
        }

        private readonly Dictionary<uint, string> _processTitles;

        public bool TryGetTitleForProcess(uint processId, out string processTitle)
        {
            return _processTitles.TryGetValue(processId, out processTitle);
        }
    }
}
