using EarTrumpet_Actions.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet_Actions.ViewModel
{
    class PartViewModelFactory
    {
        class TypeInfo
        {
            public Type Type { get; }
            public Type ConstructorType { get; }

            public TypeInfo(Type type)
            {
                Type = type;
                ConstructorType = type.GetConstructors()[0].GetParameters()[0].ParameterType;
            }
        }

        private static List<TypeInfo> s_partViewModelClasses;

        public static PartViewModel Create(Part part)
        {
            PopulateCache();

            var info = s_partViewModelClasses.First(t => t.ConstructorType == part.GetType());
            return Create(info);
        }

        public static IEnumerable<PartViewModel> Create<T>() where T : Part
        {
            PopulateCache();

            var ret = new List<PartViewModel>();

            foreach(var info in s_partViewModelClasses)
            {
                if (typeof(T).IsAssignableFrom(info.ConstructorType))
                {
                    ret.Add(Create(info));
                }
            }

            return ret;
        }

        private static PartViewModel Create(TypeInfo info)
        {
            return (PartViewModel)Activator.CreateInstance(info.Type, args: (Activator.CreateInstance(info.ConstructorType)));
        }

        private static void PopulateCache()
        {
            if (s_partViewModelClasses == null)
            {
                var partVmType = typeof(PartViewModel);
                s_partViewModelClasses = partVmType.Assembly.GetTypes().Where(t => partVmType.IsAssignableFrom(t)).Select(t => new TypeInfo(t)).ToList();
            }
        }
    }
}
