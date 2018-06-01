using EarTrumpet.Interop;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EarTrumpet.Misc
{
    sealed class WindowsTaskbar
    {
        public struct State
        {
            public Position Location;
            public RECT Size;
            public Screen ContainingScreen;
        }

        public enum Position
        {
            Top,
            Left,
            Right,
            Bottom
        }

        private const string _className = "Shell_TrayWnd";

        public static State Current
        {
            get
            {
                var appbar = new Shell32.APPBARDATA();

                var hwnd = User32.FindWindow(_className, null);

                appbar.cbSize = Marshal.SizeOf(appbar);
                appbar.uEdge = 0;
                appbar.hWnd = hwnd;
                appbar.lParam = 1;

                User32.GetWindowRect(hwnd, out RECT scaledTaskbarRect);

                var taskbarNonDPIAwareSize = Shell32.SHAppBarMessage(Shell32.ABMsg.ABM_GETTASKBARPOS, ref appbar);
                var scalingAmount = (double)(scaledTaskbarRect.Bottom - scaledTaskbarRect.Top) / (appbar.rc.Bottom - appbar.rc.Top);

                State retState = new State();
                retState.Size.Top = (int)(appbar.rc.Top * scalingAmount);
                retState.Size.Bottom = (int)(appbar.rc.Bottom * scalingAmount);
                retState.Size.Left = (int)(appbar.rc.Left * scalingAmount);
                retState.Size.Right = (int)(appbar.rc.Right * scalingAmount);

                var screen = Screen.AllScreens.FirstOrDefault(x => x.Bounds.Contains(
                    new Rectangle(
                        retState.Size.Left,
                        retState.Size.Top,
                        retState.Size.Right - retState.Size.Left,
                        retState.Size.Bottom - retState.Size.Top)
                ));

                retState.ContainingScreen = screen;
                retState.Location = Position.Bottom;

                if (screen != null)
                {
                    if (retState.Size.Bottom == screen.Bounds.Bottom && retState.Size.Top == screen.Bounds.Top)
                    {
                        retState.Location = (retState.Size.Left == screen.Bounds.Left) ? Position.Left : Position.Right;
                    }
                    if (retState.Size.Right == screen.Bounds.Right && retState.Size.Left == screen.Bounds.Left)
                    {
                        retState.Location = (retState.Size.Top == screen.Bounds.Top) ? Position.Top : Position.Bottom;
                    }
                }

                return retState;
            }
        }
    }


}