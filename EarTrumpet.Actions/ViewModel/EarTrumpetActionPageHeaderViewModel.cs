using EarTrumpet.UI.ViewModels;

namespace EarTrumpet_Actions.ViewModel
{
    public class EarTrumpetActionPageHeaderViewModel : SettingsPageHeaderViewModel
    {
        EarTrumpetActionViewModel _parent;

        public ToolbarItemViewModel[] Toolbar => _parent.Toolbar;
        public string DisplayName { get => _parent.DisplayName; set => _parent.DisplayName = value; }

        public EarTrumpetActionPageHeaderViewModel(EarTrumpetActionViewModel parent) : base(parent)
        {
            _parent = parent;
        }
    }
}