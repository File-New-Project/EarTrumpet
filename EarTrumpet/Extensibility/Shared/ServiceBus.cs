using System.Collections.Generic;

namespace EarTrumpet.Extensibility.Shared
{
    public static class ServiceBus
    {
        static Dictionary<string, object> _services = new Dictionary<string, object>();

        public static void RegisterMany(string name, object service)
        {
            if (_services.ContainsKey(name))
            {
                ((List<object>)_services[name]).Add(service);
                return;
            }

            _services[name] = new List<object>(new object[] { service });
        }

        public static List<object> GetMany(string name)
        {
            _services.TryGetValue(name, out var ret);
            if (ret == null)
            {
                ret = new List<object>();
            }
            return (List<object>)ret;
        }
    }
}
