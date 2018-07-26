using System;

namespace EarTrumpet.Extensibility
{
    public interface IValue<T>
    {
        T Value { get; set; }

        string DisplayName { get; }

        String Id { get; }
    }
}
