using System;

namespace EarTrumpet.UI.Helpers
{
    public class NavigationCookie
    {
        Action _action;
        public NavigationCookie(Action action)
        {
            _action = action;
        }

        public void Execute()
        {
            _action.Invoke();
        }
    }
}
