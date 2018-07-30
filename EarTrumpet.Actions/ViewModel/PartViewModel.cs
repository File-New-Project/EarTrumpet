using EarTrumpet.UI.ViewModels;
using EarTrumpet_Actions.DataModel;
using EarTrumpet_Actions.DataModel.Actions;
using EarTrumpet_Actions.DataModel.Conditions;
using EarTrumpet_Actions.DataModel.Triggers;
using System;
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
                if (_currentDescription != value)
                {
                    _currentDescription = value;
                    RaisePropertyChanged(nameof(CurrentDescription));
                }
            }
        }


        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    RaisePropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public string Verb

        {
            get
            {
                if (Part is BaseTrigger)
                {
                    return Properties.Resources.TriggerVerbText;
                }
                else if (Part is BaseCondition)
                {
                    return Properties.Resources.ConditionVerbText;
                }
                else if (Part is BaseAction)
                {
                    return Properties.Resources.ActionVerbText;
                }
                else throw new NotImplementedException();
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