namespace EarTrumpet.UI.ViewModels
{
    class MIDIControlWizardViewModel : BindableBase
    {
        public string Title { get; private set; }
        
        public MIDIControlWizardViewModel(string title)
        {
            Title = title;
        }
    }
}