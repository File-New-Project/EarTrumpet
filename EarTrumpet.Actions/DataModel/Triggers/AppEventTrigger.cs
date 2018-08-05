using EarTrumpet_Actions.DataModel.Enum;
using System.Collections.Generic;

namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class AppEventTrigger : BaseTrigger, IPartWithDevice, IPartWithApp
    {
        public Device Device { get; set; }
        public App App { get; set; }
        public AudioAppEventKind Option { get; set; }
        
        public AppEventTrigger()
        {
            Description = Properties.Resources.AppEventTriggerDescriptionText;
            Options = new List<OptionData>(new OptionData[]{ new OptionData(new List<Option>
                {
                    new Option(Properties.Resources.AudioAppEventKindAddedText, AudioAppEventKind.Added),
                    new Option(Properties.Resources.AudioAppEventKindRemovedText, AudioAppEventKind.Removed),
                    new Option(Properties.Resources.AudioAppEventKindMutedText, AudioAppEventKind.Muted),
                    new Option(Properties.Resources.AudioAppEventKindUnmutedText, AudioAppEventKind.Unmuted),
                    new Option(Properties.Resources.AudioAppEventKindBeginPlaySoundText, AudioAppEventKind.PlayingSound),
                    new Option(Properties.Resources.AudioAppEventKindEndsPlaySoundText, AudioAppEventKind.NotPlayingSound),
                },
                (newValue) => Option = (AudioAppEventKind)newValue.Value,
                () => Option) });
        }

        public override string Describe() => string.Format(Properties.Resources.AppEventTriggerDescribeFormatText, App, Device, Options[0].DisplayName);
    }
}
