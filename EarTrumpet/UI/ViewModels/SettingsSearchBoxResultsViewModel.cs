using System;
using System.Collections.ObjectModel;

namespace EarTrumpet.UI.ViewModels
{
    class SettingsSearchBoxResultsViewModel : BindableBase
    {
        public ObservableCollection<SettingsSearchBoxResultsItemViewModel> Results { get; } = new ObservableCollection<SettingsSearchBoxResultsItemViewModel>();

        public bool IsDone { get; private set; }

        public SettingsSearchBoxResultsViewModel(SettingsViewModel viewModel, string text, Action beforeInvoke)
        {
            text = text.ToLower();

            foreach(var cat in viewModel.Categories)
            {
                foreach(var page in cat.Pages)
                {
                    if (page.Title.ToLower().Contains(text))
                    {
                        Results.Add(new SettingsSearchBoxResultsItemViewModel
                        {
                            DisplayName = page.Title,
                            Glyph = page.Glyph,
                            Invoke = () =>
                            {
                                beforeInvoke();
                                viewModel.InvokeSearchResult(cat, page);
                            }
                        });
                    }
                }

                if (Results.Count > 5)
                {
                    return;
                }
            }

            if (Results.Count == 0)
            {
                Results.Add(new SettingsSearchBoxResultsItemViewModel { DisplayName = Properties.Resources.SearchBoxNoResultsText, Invoke = () => {
                    beforeInvoke();
                } });
            }
        }

        public void Invoked(object sender, object viewModel)
        {
            IsDone = true;
            RaisePropertyChanged(nameof(IsDone));
            ((SettingsSearchBoxResultsItemViewModel)viewModel).Invoke();
        }
    }
}
