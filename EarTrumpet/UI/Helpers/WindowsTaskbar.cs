using EarTrumpet.Interop;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EarTrumpet.UI.Helpers
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
            Left,
            Top,
            Right,
            Bottom
        }

        public static State Current
        {
            get
            {
                var state = new State();
                var hWnd = User32.FindWindow("Shell_TrayWnd", null);
                var appBarData = new APPBARDATA
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA)),
                    hWnd = hWnd
                };

                // Use SHAppBarMessage because it understands Taskbar AutoHide 
                // state (the window is positioned across screens).  
                // Otherwise fallback to GetWindowRect for non-standard shells.
                if (Shell32.SHAppBarMessage(AppBarMessage.GetTaskbarPos, ref appBarData))
                {
                    state.Size = appBarData.rect;
                    state.Location = (Position)appBarData.uEdge;
                    state.ContainingScreen = ScreenFromRect(state);
                }
                else
                {
                    User32.GetWindowRect(hWnd, out state.Size);
                    var screen = ScreenFromRect(state);
                    state.ContainingScreen = screen;

                    if (screen != null)
                    {
                        if (state.Size.Bottom == screen.Bounds.Bottom && state.Size.Top == screen.Bounds.Top)
                        {
                            state.Location = (state.Size.Left == screen.Bounds.Left) ? Position.Left : Position.Right;
                        }
                        if (state.Size.Right == screen.Bounds.Right && state.Size.Left == screen.Bounds.Left)
                        {
                            state.Location = (state.Size.Top == screen.Bounds.Top) ? Position.Top : Position.Bottom;
                        }
                    }
                }

                return state;
            }
        }

        private static Screen ScreenFromRect(State state)
        {
            return Screen.AllScreens.FirstOrDefault(x => x.Bounds.Contains(
                new Rectangle(
                state.Size.Left,
                state.Size.Top,
                state.Size.Right - state.Size.Left,
                state.Size.Bottom - state.Size.Top)
            ));
        }
    }
}