using System;

namespace EarTrumpet_Actions.DataModel.Actions
{
    public class DelayAction : BaseAction
    {
        public TimeSpan Time { get; set; }

        public override void Invoke()
        {
            throw new NotImplementedException();
        }

        public override void Loaded()
        {
            throw new NotImplementedException();
        }

        public override string ToString() => "Delay for a period before the next action";
    }
}
