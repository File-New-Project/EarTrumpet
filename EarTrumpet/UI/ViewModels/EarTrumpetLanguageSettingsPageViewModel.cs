using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EarTrumpet.UI.Services;
namespace EarTrumpet.UI.ViewModels
{
    //hamed
    class EarTrumpetLanguageSettingsPageViewModel : SettingsPageViewModel
    {
        public Lang Language
        {
            get => languageList.Find(x=>x.L== SettingsService.Language);
            set => SettingsService.Language = value.L;
        }
        public string Titles
        {
            get => languageList.Find(x => x.L == SettingsService.Language).Title;
            set { }
        }
        public List<Lang> LanguageList { get => languageList; set => languageList = value; }

        private List<Lang> languageList = new List<Lang>();
        public EarTrumpetLanguageSettingsPageViewModel() : base(null)
        {
            Title = Properties.Resources.LanguageSettingsPageText;
            Glyph = "\xE775";
            languageList.AddRange(new [] { new Lang() { L = "Auto", Title = "Auto" },
                new Lang() { L = "ar", Title = "Arabic" },
            new Lang() { L = "fa-IR", Title = "Farsi" },
            new Lang() { L = "en-US", Title = "English" },
            new Lang() { L = "es-ES", Title = "Spanish" },
            new Lang() { L = "he-IL", Title = "Hebrew" },
            new Lang() { L = "it", Title = "italia" },
            new Lang() { L = "fr-FR", Title = "French" },
            new Lang() { L = "ja-JP", Title = "japon" },
            new Lang() { L = "ro", Title = "Romanian" },});

            //    <ComboBoxItem Tag="de-DE" Content="de-DE" />
            //    <ComboBoxItem Tag="ko-KR" Content="ko-KR" />
            //    <ComboBoxItem Tag="nb-NO" Content="nb-NO" />
            //    <ComboBoxItem Tag="pt-BR" Content="pt-BR" />
            //    <ComboBoxItem Tag="pl-PL" Content="pl-PL" />
            //    <ComboBoxItem Tag="pt-PT" Content="pt-PT" />
            //    <ComboBoxItem Tag="tr-TR" Content="tr-TR" />
            //    <ComboBoxItem Tag="uk-UA" Content="uk-UA" />
            //    <ComboBoxItem Tag="zh-CN" Content="zh-CN" />
            //    <ComboBoxItem Tag="zh-TW" Content="zh-TW" />
        }
    }
    public class Lang
    {
        private string title;
        public string Title { get => title; set => title = value; }
        private string l;
        public string L { get => l; set => l = value; }
    }
}
