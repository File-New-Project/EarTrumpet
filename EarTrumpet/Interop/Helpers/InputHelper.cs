using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EarTrumpet.Interop.Helpers
{
    class InputHelper
    {
        public static void RegisterForMouseInput(IntPtr handle)
        {
            var data = new User32.RAWINPUTDEVICE
            {
                usUsagePage = User32.HidUsagePage.GENERIC,
                usUsage = User32.HidUsage.Mouse,
                dwFlags = User32.RIDEV_INPUTSINK,
                hwndTarget = handle
            };

            if (!RegisterRawInputDevices(data))
            {
                Trace.WriteLine($"InputHelper UnregisterForMouseInput: {Marshal.GetLastWin32Error()}");
            }
        }

        public static void UnregisterForMouseInput()
        {
            var data = new User32.RAWINPUTDEVICE
            {
                usUsagePage = User32.HidUsagePage.GENERIC,
                usUsage = User32.HidUsage.Mouse,
                dwFlags = User32.RIDEV_REMOVE
            };

            if (!RegisterRawInputDevices(data))
            {
                Trace.WriteLine($"InputHelper UnregisterForMouseInput: {Marshal.GetLastWin32Error()}");
            }
        }

        private static bool RegisterRawInputDevices(User32.RAWINPUTDEVICE data)
        {
            var devicePtr = Marshal.AllocHGlobal(Marshal.SizeOf(data));
            Marshal.StructureToPtr(data, devicePtr, false);
            bool ret = User32.RegisterRawInputDevices(devicePtr, 1, (uint)Marshal.SizeOf(data));
            Marshal.FreeHGlobal(devicePtr);
            return ret;
        }

        public static bool ProcessMouseInputMessage(IntPtr lParam, ref System.Drawing.Point cursorPosition, out int wheelDelta)
        {
            wheelDelta = 0;
            bool isApplicableMouseMessage = false;

            var header = new User32.RAWINPUTHEADER();
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
                        isApplicableMouseMessage = true;

                        if (rawInput.mouse.usFlags.HasFlag(User32.RAWMOUSE_FLAGS.MOUSE_MOVE_ABSOLUTE))
                        {
                            int width, height;
                            if (rawInput.mouse.usFlags.HasFlag(User32.RAWMOUSE_FLAGS.MOUSE_VIRTUAL_DESKTOP))
                            {
                                width = User32.GetSystemMetricsForDpi(User32.SystemMetrics.SM_CXVIRTUALSCREEN, WindowsTaskbar.Dpi);
                                height = User32.GetSystemMetricsForDpi(User32.SystemMetrics.SM_CYVIRTUALSCREEN, WindowsTaskbar.Dpi);
                            }
                            else
                            {
                                width = Screen.PrimaryScreen.Bounds.Width;
                                height = Screen.PrimaryScreen.Bounds.Height;
                            }

                            cursorPosition.X = rawInput.mouse.lLastX / 65535 * width;
                            cursorPosition.Y = rawInput.mouse.lLastY / 65535 * height;
                        }
                        else
                        {
                            cursorPosition.X += rawInput.mouse.lLastX;
                            cursorPosition.Y += rawInput.mouse.lLastY;
                        }

                        if ((rawInput.mouse.usButtonFlags & User32.RI_MOUSE_WHEEL) == User32.RI_MOUSE_WHEEL)
                        {
                            wheelDelta = rawInput.mouse.usButtonData;
                        }
                    }
                }
                Marshal.FreeHGlobal(dataPtr);
            }
            return isApplicableMouseMessage;
        }
    }
}
