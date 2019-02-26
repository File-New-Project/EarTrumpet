using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EarTrumpet.Actions.ViewModel
{
    class OptionViewModel : BindableBase, IOptionViewModel
    {
        public ObservableCollection<Option> All { get; }

        public Option Selected
        {
            get
            {
                var value = (int)_target.GetType().GetProperty(_property).GetValue(_target);
                return All.First(o => o.Value.Equals(value));
            }
            set
            {
                if (Selected != value)
                {
                    _target.GetType().GetProperty(_property).SetValue(_target, value.Value);
                    RaisePropertyChanged(nameof(Selected));
                }
            }
        }

        private readonly object _target;
        private readonly string _property;

        public OptionViewModel(object target, string property)
        {
            _target = target;
            _property = property;

            var propType = target.GetType().GetProperty(property).PropertyType;
            All = new ObservableCollection<Option>(Enum.GetValues(propType).Cast<int>().Select(v => 
            new Option(GetLocalizedString(Enum.GetName(propType, v)), v)));
        }

        public override string ToString()
        {
            return Selected?.DisplayName;
        }

        private string GetLocalizedString(string name)
        {
            var propType = _target.GetType().GetProperty(_property).PropertyType;
            var resourceName = $"{propType.Name}_{name}";
            var ret = Properties.Resources.ResourceManager.GetString(resourceName);
            if (string.IsNullOrWhiteSpace(ret))
            {
                throw new NotImplementedException(resourceName);
            }
            return ret;
        }
    }
}
