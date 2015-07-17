using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EarTrumpet.Services
{
    public sealed class TaskbarService
    {
        private const string ClassName = "Shell_TrayWnd";

        public static Rectangle TaskbarPostionRect
        {
            get
            {
                IntPtr taskbarHandle = User32.FindWindow(TaskbarService.ClassName, null);

                RECT r = new RECT();
                User32.GetWindowRect(taskbarHandle, ref r);

                return Rectangle.FromLTRB(r.left, r.top, r.right, r.bottom);
            }
        }

        public static TaskbarPosition TaskbarPosition
        {
            get
            {
                var rect = TaskbarPostionRect;
                if (rect.Bottom == System.Windows.SystemParameters.PrimaryScreenHeight && rect.Top == 0)
                {
                    return (rect.Left == 0) ? TaskbarPosition.Left : TaskbarPosition.Right;
                }
                if (rect.Right == System.Windows.SystemParameters.PrimaryScreenWidth && rect.Left == 0)
                {
                    return (rect.Top == 0) ? TaskbarPosition.Top : TaskbarPosition.Bottom;
                }                
                return TaskbarPosition.Bottom;
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
