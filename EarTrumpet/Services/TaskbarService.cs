using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace EarTrumpet.Services
{
    public sealed class TaskbarService
    {
        private const string ClassName = "Shell_TrayWnd";

        public static Rectangle TaskbarPostionRect
        {
            get
            {
                var taskbarHandle = User32.FindWindow(ClassName, null);

                var r = new RECT();
                User32.GetWindowRect(taskbarHandle, ref r);

                return Rectangle.FromLTRB(r.left, r.top, r.right, r.bottom);
            }
        }

        public static TaskbarPosition TaskbarPosition
        {
            get
            {
                var rect = TaskbarPostionRect;
                var screen = TaskbarScreen;
                if (screen == null) return TaskbarPosition.Bottom;

                if (rect.Bottom == screen.Bounds.Bottom && rect.Top == screen.Bounds.Top)
                {
                    return (rect.Left == screen.Bounds.Left) ? TaskbarPosition.Left : TaskbarPosition.Right;
                }
                if (rect.Right == screen.Bounds.Right && rect.Left == screen.Bounds.Left)
                {
                    return (rect.Top == screen.Bounds.Top) ? TaskbarPosition.Top : TaskbarPosition.Bottom;
                }
                return TaskbarPosition.Bottom;
            }
        }

        public static Screen TaskbarScreen
        {
            get
            {
                var rect = TaskbarPostionRect;
                return Screen.AllScreens.FirstOrDefault(x => x.Bounds.Contains(rect));
            }
        }
    }

    public static class User32
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    public enum TaskbarPosition
    {
        Top,
        Left,
        Right,
        Bottom
    }
}