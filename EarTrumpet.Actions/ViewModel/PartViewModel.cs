using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using System.Windows.Input;

namespace EarTrumpet_Actions.ViewModel
{
    public class PartViewModel : BindableBase
    {
        public Part Part { get; }
        public string Description => Part.Description;

        private string _currentDescription;
        public string CurrentDescription
        {
            get => _currentDescription;
            set
            {
                // HACK: We need to raise here because the format string won't have changed
                _currentDescription = "";
                RaisePropertyChanged(nameof(CurrentDescription));
                _currentDescription = value;
                RaisePropertyChanged(nameof(CurrentDescription));
            }
        }

        public ICommand Open { get; set; }
        public ICommand Remove { get; set; }

        public PartViewModel(Part part)
        {
            Part = part;

            UpdateDescription();
        }

        protected void UpdateDescription()
        {
            CurrentDescription = Part.Describe();
        }
    }
}