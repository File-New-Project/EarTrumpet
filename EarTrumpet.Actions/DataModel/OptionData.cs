using System;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet_Actions.DataModel
{
    public class OptionData
    {
        public List<Option> Options { get; }

        public Option Selected
        {
            get => Options.FirstOrDefault(o => o.Value.Equals(GetSelected()));
            set => SetSelected(value);
        }

        public Action<Option> SetSelected { get; }
        public Func<object> GetSelected { get; }

        public string DisplayName => Selected?.DisplayName;

        public OptionData(IEnumerable<Option> options, Action<Option> setSelected, Func<object> getSelected)
        {
            Options = options.ToList();
            SetSelected = setSelected;
            GetSelected = getSelected;
        }
    }
}
