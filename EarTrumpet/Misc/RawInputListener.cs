using EarTrumpet.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace EarTrumpet.Misc
{
    class RawInputListener
    {
        public event EventHandler<int> MouseWheel;

        private readonly IntPtr _hwnd;

        public RawInputListener(Window window)
        {
            _hwnd = new WindowInteropHelper(window).Handle;
            HwndSource.FromHwnd(_hwnd).AddHook(WndProc);
        }

        public void Start()
        {
            RegisterForRawMouseInput(User32.RIDEV_INPUTSINK);
        }

        public void Stop()
        {
            RegisterForRawMouseInput(User32.RIDEV_REMOVE);
        }

        private void RegisterForRawMouseInput(uint flags)
        {
            User32.RAWINPUTDEVICE mouseRawDevice = new User32.RAWINPUTDEVICE();
            mouseRawDevice.usUsagePage = (ushort)User32.HidUsagePage.GENERIC;
            mouseRawDevice.usUsage = (ushort)User32.HidUsage.Mouse;
            mouseRawDevice.dwFlags = flags;
            mouseRawDevice.hwndTarget = (flags == User32.RIDEV_REMOVE ? IntPtr.Zero : _hwnd);

            var devicePtr = Marshal.AllocHGlobal(Marshal.SizeOf(mouseRawDevice));
            Marshal.StructureToPtr(mouseRawDevice, devicePtr, false);

            if (User32.RegisterRawInputDevices(devicePtr, 1, (uint)Marshal.SizeOf(mouseRawDevice)) == false)
            {
                int err = Marshal.GetLastWin32Error();
                Debug.WriteLine($"Couldn't register for raw input: {flags} {err} {_hwnd}");
            }

            Marshal.FreeHGlobal(devicePtr);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == User32.WM_INPUT)
            {
                User32.RAWINPUTHEADER header = new User32.RAWINPUTHEADER();
                var headerSize = (uint)Marshal.SizeOf(header);

                uint bufferSize = 0;
                uint res = User32.GetRawInputData(lParam, User32.RID_INPUT, IntPtr.Zero, ref bufferSize, headerSize);
                if (res == 0)
                {
                    var dataPtr = Marshal.AllocHGlobal((int)bufferSize);
                    uint writtenBytes = User32.GetRawInputData(lParam, User32.RID_INPUT, dataPtr, ref bufferSize, headerSize);
                    if (writtenBytes == bufferSize)
                    {
                        var rawInput = Marshal.PtrToStructure<User32.RAWINPUT>(dataPtr);
                        if (rawInput.header.dwType == User32.RIM_TYPEMOUSE)
                        {
                            if ((rawInput.mouse.usButtonFlags & User32.RI_MOUSE_WHEEL) == User32.RI_MOUSE_WHEEL)
                            {
                                MouseWheel?.Invoke(this, rawInput.mouse.usButtonData);
                            }
                        }
                    }
                    Marshal.FreeHGlobal(dataPtr);
                }
            }
            return IntPtr.Zero;
        }
    }
}
