using System.Collections.ObjectModel;

namespace EarTrumpet.UI.ViewModels
{
    class SettingsSearchBoxResultsViewModel
    {
        public ObservableCollection<SettingsSearchBoxResultsItemViewModel> Results { get; } = new ObservableCollection<SettingsSearchBoxResultsItemViewModel>();

        public SettingsSearchBoxResultsItemViewModel Selected
        {
            get => Results.Count > 0 ? Results[0] : null;
            set
            {
                value.Invoke();
            }
        }

        public SettingsSearchBoxResultsViewModel(SettingsViewModel viewModel, string text)
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
                                viewModel.InvokeSearchResult(cat, page);
                            }
                        });
                    }
                }

                if (Results.Count > 10)
                {
                    return;
                }
            }

            if (Results.Count == 0)
            {
                Results.Add(new SettingsSearchBoxResultsItemViewModel { DisplayName = Properties.Resources.SearchBoxNoResultsText });
            }
        }
    }
}
