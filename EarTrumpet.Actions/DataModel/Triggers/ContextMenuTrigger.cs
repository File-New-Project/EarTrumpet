namespace EarTrumpet_Actions.DataModel.Triggers
{
    public class ContextMenuTrigger : BaseTrigger
    {
        public ContextMenuTrigger()
        {
            Description = Properties.Resources.ContextMenuTriggerDescriptionText;
        }

        public override string Describe() => Properties.Resources.ContextMenuTriggerCurrentDescriptionText;
    }
}
