using System;
using System.Drawing;

namespace EarTrumpet.UI.Helpers
{
    public interface IIconSource
    {
        event Action<IIconSource> Changed;
        Icon Current { get; }
        void OnMouseOverChanged(bool isMouseOver);
        void CheckForUpdate();
    }
}
