using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EarTrumpet.Services
{
    public sealed class TaskbarService
    {
        private const string ClassName = "Shell_TrayWnd";

        public static TaskbarState GetWinTaskbarState()
        {
            TaskbarState retState = new TaskbarState();
            var hwnd = User32.FindWindow(ClassName, null);

            User32.GetWindowRect(hwnd, out retState.TaskbarSize);

            var screen = Screen.AllScreens.FirstOrDefault(x => x.Bounds.Contains(
                            new Rectangle(
                                retState.TaskbarSize.left, 
                                retState.TaskbarSize.top,
                                retState.TaskbarSize.right - retState.TaskbarSize.left, 
                                retState.TaskbarSize.bottom - retState.TaskbarSize.top)
                         ));

            retState.TaskbarPosition = TaskbarPosition.Bottom;

            if (screen != null)
            {
                if (retState.TaskbarSize.bottom == screen.Bounds.Bottom && retState.TaskbarSize.top == screen.Bounds.Top)
                {
                    retState.TaskbarPosition = (retState.TaskbarSize.left == screen.Bounds.Left) ? TaskbarPosition.Left : TaskbarPosition.Right;
                }
                if (retState.TaskbarSize.right == screen.Bounds.Right && retState.TaskbarSize.left == screen.Bounds.Left)
                {
                    retState.TaskbarPosition = (retState.TaskbarSize.top == screen.Bounds.Top) ? TaskbarPosition.Top : TaskbarPosition.Bottom;
                }
            }               

            return retState;
        }
    }

    public static class User32
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TaskbarState
    {
        public TaskbarPosition TaskbarPosition;
        public RECT TaskbarSize;
    }

    public enum TaskbarPosition
    {
        Top,
        Left,
        Right,
        Bottom
    }
}