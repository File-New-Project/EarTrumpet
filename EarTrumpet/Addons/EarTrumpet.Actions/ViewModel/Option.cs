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

        public override bool Equals(object obj)
        {
            return (obj as Option).DisplayName.Equals(DisplayName, StringComparison.Ordinal);
        }

        public bool Equals(Option other) => Equals(other);

        public override int GetHashCode()
        {
            return HashCode.Combine(DisplayName, Value);
        }
    }
}
