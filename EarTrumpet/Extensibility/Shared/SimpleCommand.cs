using System.Windows.Input;

namespace EarTrumpet.Extensibility.Shared
{
    public class SimpleCommand
    {
        public ICommand Command { get; set; }
        public string DisplayName { get; set; }
        public string Id { get; set; }
    }
}
