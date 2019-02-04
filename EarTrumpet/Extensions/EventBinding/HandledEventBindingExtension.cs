using System;
using System.Reflection;
using System.Windows.Markup;

namespace EarTrumpet.Extensions.EventBinding
{
    // {Event:HandledBinding}
    public class HandledBindingExtension : MarkupExtension
    {
        public HandledBindingExtension() { }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (provideValueTarget != null)
            {
                Type delegateType = null;
                if ((provideValueTarget.TargetProperty as EventInfo) != null)
                {
                    delegateType = (provideValueTarget.TargetProperty as EventInfo).EventHandlerType;
                }
                else
                {
                    delegateType = (provideValueTarget.TargetProperty as MethodInfo).GetParameters()[1].ParameterType;
                }

                return Delegate.CreateDelegate(delegateType,
                    this,
                    GetType().GetMethod(nameof(OnEvent), BindingFlags.NonPublic | BindingFlags.Instance));
            }
            return null;
        }

        void OnEvent(object sender, object args)
        {
            args.GetType().GetProperty("Handled").SetValue(args, true);
        }
    }
}