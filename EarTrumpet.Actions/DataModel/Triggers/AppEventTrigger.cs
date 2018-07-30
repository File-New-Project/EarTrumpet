using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public enum AudioAppEventKind
    {
        Added,
        Removed,
        PlayingSound,
        NotPlayingSound,
        Muted,
        Unmuted,
    }

    public class AppEventTrigger : BaseTrigger, IPartWithDevice, IPartWithApp
    {
        public Device Device { get; set; }
        public App App { get; set; }
        public AudioAppEventKind Option { get; set; }
        
        public AppEventTrigger()
        {
            Description = "When an app session is (added, removed, plays sound, ...)";
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                    new Option("is added", AudioAppEventKind.Added),
                    new Option("is removed", AudioAppEventKind.Removed),
                    new Option("is muted", AudioAppEventKind.Muted),
                    new Option("is unmuted", AudioAppEventKind.Unmuted),
                    new Option("begins playing sound", AudioAppEventKind.PlayingSound),
                    new Option("stops playing sound", AudioAppEventKind.NotPlayingSound),
                },
                (newValue) => Option = (AudioAppEventKind)newValue.Value,
                () => Option) });


        }


        public override string Describe() => $"When {App} on {Device} {Options[0].DisplayName}";

    }
}
