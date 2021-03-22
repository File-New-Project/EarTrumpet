using System;

namespace EarTrumpet.Actions.ViewModel
{
    public class Option : IEquatable<Option>
    {
        public string DisplayName { get; set; }
        public object Value { get; set; }

        public Option(string displayName, object value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public bool Equals(Option other)
        {
            return other.DisplayName.Equals(DisplayName);
        }
    }
}
