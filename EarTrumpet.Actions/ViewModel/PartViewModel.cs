using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Conditions;
using EarTrumpet_Actions.DataModel.Triggers;
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
                _currentDescription = "empty";
                RaisePropertyChanged(nameof(CurrentDescription));
                _currentDescription = value;
                RaisePropertyChanged(nameof(CurrentDescription));
            }
        }

        private string _additionalText;
        public string AdditionalText
        {
            get => _additionalText;
            set
            {
                _additionalText = value;
                RaisePropertyChanged(nameof(AdditionalText));
            }
        }

        public bool IsShowingAdditionalText
        {
            get => !string.IsNullOrWhiteSpace(_additionalText);
            set
            {
                if (value)
                {
                    if (Part is BaseTrigger)
                    {
                        _additionalText = Properties.Resources.TriggerAdditionalText;
                    }
                    else if (Part is BaseCondition)
                    {
                        _additionalText = Properties.Resources.ConditionAdditionalText;
                    }
                    else
                    {
                        _additionalText = Properties.Resources.ActionAdditionalText;
                    }
                }
                else
                {
                    _additionalText = null;
                }
            }
        }

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