using System.Windows.Media;

namespace EarTrumpet.UI.Themes
{
    public interface IResolveColor
    {
        Color Resolve(IResolveColorOptions data);
    }
}
