namespace EarTrumpet.Extensions.EventBinding
{
    // {Event:HandledBinding}
    public class HandledBindingExtension : BindingExtension
    {
        protected override void OnEvent(object sender, object args)
        {
            args.GetType().GetProperty("Handled").SetValue(args, true);
        }
    }
}