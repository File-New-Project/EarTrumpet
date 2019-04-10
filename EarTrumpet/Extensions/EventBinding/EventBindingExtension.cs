using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace EarTrumpet.Extensions.EventBinding
{
    // {Event:Binding Some.Object.FunctionName}
    public class BindingExtension : MarkupExtension
    {
        public string Path { get; set; }

        public BindingExtension() { }
        public BindingExtension(string path) { Path = path; }

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

        protected virtual void OnEvent(object sender, object args)
        {
            var target = ResolvePropertyPath((sender as FrameworkElement).DataContext, Path, out string functionName);
            target.GetType().GetMethod(functionName).Invoke(target, new object[] { sender, args });
        }

        private object ResolvePropertyPath(object target, string path, out string functionName)
        {
            var parts = path.Split('.');
            functionName = parts.Last();
            foreach (var segment in parts.Take(parts.Length - 1))
            {
                target = target.GetType().GetProperty(segment).GetValue(target);
            }
            return target;
        }
    }
}