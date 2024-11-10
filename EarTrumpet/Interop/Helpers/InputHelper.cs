using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.UI.Input;
using EarTrumpet.Extensions;

namespace EarTrumpet.Interop.Helpers;

internal class InputHelper
{
    public static void RegisterForMouseInput(IntPtr handle)
    {
        unsafe
        {
            var data = new RAWINPUTDEVICE
            {
                usUsagePage = PInvoke.HID_USAGE_PAGE_GENERIC,
                usUsage = PInvoke.HID_USAGE_GENERIC_MOUSE,
                dwFlags = RAWINPUTDEVICE_FLAGS.RIDEV_INPUTSINK,
                hwndTarget = new HWND(handle.ToPointer())
            };

            if (!RegisterRawInputDevices(data))
            {
                Trace.WriteLine($"InputHelper RegisterForMouseInput: {Marshal.GetLastWin32Error()}");
            }
        }
    }

    public static void UnregisterForMouseInput()
    {
        var data = new RAWINPUTDEVICE
        {
            usUsagePage = PInvoke.HID_USAGE_PAGE_GENERIC,
            usUsage = PInvoke.HID_USAGE_GENERIC_MOUSE,
            dwFlags = RAWINPUTDEVICE_FLAGS.RIDEV_REMOVE
        };

        if (!RegisterRawInputDevices(data))
        {
            Trace.WriteLine($"InputHelper UnregisterForMouseInput: {Marshal.GetLastWin32Error()}");
        }
    }

    private static bool RegisterRawInputDevices(RAWINPUTDEVICE data)
    {
        return PInvoke.RegisterRawInputDevices([data], (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
    }

    public static unsafe bool ProcessMouseInputMessage(IntPtr lParam, ref System.Drawing.Point cursorPosition, out int wheelDelta)
    {
        wheelDelta = 0;
        var rawInputHandle = new HRAWINPUT(lParam.ToPointer());
        var isApplicableMouseMessage = false;
        
        var headerSize = (uint)Marshal.SizeOf<RAWINPUTHEADER>();
        uint bufferSize = 0;
        uint result = 1;
        unsafe
        {
            result = PInvoke.GetRawInputData(rawInputHandle, RAW_INPUT_DATA_COMMAND_FLAGS.RID_INPUT, null, ref bufferSize, headerSize);
        }

        if (result == 0)
        {
            Span<byte> dataBuffer = stackalloc byte[(int)bufferSize];
            var writtenBytes = 0U;
            unsafe
            {
                fixed (byte* dataPtr = dataBuffer)
                {
                    writtenBytes = PInvoke.GetRawInputData(rawInputHandle, RAW_INPUT_DATA_COMMAND_FLAGS.RID_INPUT, dataPtr, ref bufferSize, headerSize);
                }
            }
            if (writtenBytes == bufferSize)
            {
                var rawInput = MemoryMarshal.Read<RAWINPUT>(dataBuffer);
                if (rawInput.header.dwType == (uint)RID_DEVICE_INFO_TYPE.RIM_TYPEMOUSE)
                {
                    isApplicableMouseMessage = true;

                    if (rawInput.data.mouse.usFlags.HasFlag(MOUSE_STATE.MOUSE_MOVE_ABSOLUTE))
                    {
                        int width, height;
                        if (rawInput.data.mouse.usFlags.HasFlag(MOUSE_STATE.MOUSE_VIRTUAL_DESKTOP))
                        {
                            width = PInvoke.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN, WindowsTaskbar.Dpi);
                            height = PInvoke.GetSystemMetricsForDpi(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN, WindowsTaskbar.Dpi);
                        }
                        else
                        {
                            width = Screen.PrimaryScreen.Bounds.Width;
                            height = Screen.PrimaryScreen.Bounds.Height;
                        }

                        cursorPosition.X = rawInput.data.mouse.lLastX / 65535 * width;
                        cursorPosition.Y = rawInput.data.mouse.lLastY / 65535 * height;
                    }
                    else
                    {
                        cursorPosition.X += rawInput.data.mouse.lLastX;
                        cursorPosition.Y += rawInput.data.mouse.lLastY;
                    }

                    if (rawInput.data.mouse.Anonymous.Anonymous.usButtonFlags.HasFlag(PInvoke.RI_MOUSE_WHEEL))
                    {
                        // RI_MOUSE_WHEEL: -/+ wheel delta is stored in usButtonData
                        wheelDelta = (short)rawInput.data.mouse.Anonymous.Anonymous.usButtonData;
                    }
                }
            }
        }
        return isApplicableMouseMessage;
    }
}
