using System.Collections.Generic;

namespace EarTrumpet.Extensibility.Shared
{
    public static class ServiceBus
    {
        static Dictionary<string, object> _services = new Dictionary<string, object>();

        public static void Register(string name, object service)
        {
            _services[name] = service;
        }

        public static void Unregister(string name, object service)
        {
            _services[name] = null;
        }

        public static object Get(string name)
        {
            return _services[name];
        }

        public static bool Exists(string name) => Get(name) != null;
    }
}
