using EarTrumpet.Interop;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EarTrumpet.UI.Misc
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

        public static State Current
        {
            get
            {
                var state = new State();
                var hwnd = User32.FindWindow("Shell_TrayWnd", null);
                User32.GetWindowRect(hwnd, out state.Size);

                var screen = Screen.AllScreens.FirstOrDefault(x => x.Bounds.Contains(
                    new Rectangle(
                        state.Size.Left,
                        state.Size.Top,
                        state.Size.Right - state.Size.Left,
                        state.Size.Bottom - state.Size.Top)
                ));

                state.ContainingScreen = screen;
                state.Location = Position.Bottom;

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

                return state;
            }
        }
    }
}