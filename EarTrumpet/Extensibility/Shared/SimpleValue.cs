using System;

namespace EarTrumpet.Extensibility.Shared
{
    public class SimpleValue<T> : IValue<T>
    {
        public T Value
        {
            get => _get.Invoke();
            set => _set.Invoke(value);
        }

        public string Id { get; }

        public string DisplayName { get; }

        private Func<T> _get;
        private Action<T> _set;

        public SimpleValue(string id, string displayName, Func<T> get, Action<T> set)
        {
            Id = id;
            DisplayName = displayName;
            _get = get;
            _set = set;
        }
    }
}
