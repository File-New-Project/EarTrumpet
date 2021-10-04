using EarTrumpet.Actions.DataModel;
using EarTrumpet.Actions.DataModel.Serialization;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace EarTrumpet.Actions.ViewModel
{
    public class PartViewModel : BindableBase
    {
        public Part Part { get; }
        public string AddText => ResolveResource("AddText");
        public virtual string LinkText => ResolveResource("LinkText");

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
                        AdditionalText = Properties.Resources.TriggerAdditionalText;
                    }
                    else if (Part is BaseCondition)
                    {
                        AdditionalText = Properties.Resources.ConditionAdditionalText;
                    }
                    else
                    {
                        AdditionalText = Properties.Resources.ActionAdditionalText;
                    }
                }
                else
                {
                    AdditionalText = null;
                }
            }
        }

        public ICommand Remove { get; set; }

        public PartViewModel(Part part)
        {
            Part = part;
        }

        protected void Attach(INotifyPropertyChanged obj)
        {
            obj.PropertyChanged += (s, e) =>
            {
                RaisePropertyChanged(e.PropertyName);
                RaisePropertyChanged(nameof(LinkText));
            };
        }

        private string ResolveResource(string suffix)
        {
            var res = $"{Part.GetType().Name}_{suffix}";
            var ret = Properties.Resources.ResourceManager.GetString(res);
            if (string.IsNullOrWhiteSpace(ret))
            {
                throw new NotImplementedException($"Missing resource: {res}");
            }
            return ret;
        }
    }
}