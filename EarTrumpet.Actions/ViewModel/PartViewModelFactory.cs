using EarTrumpet.Actions.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EarTrumpet.Actions.ViewModel
{
    class PartViewModelFactory
    {
        class TypeInfo
        {
            public Type Type { get; set; }
            public Type ConstructorType { get; set; }
        }

        private static List<TypeInfo> s_partViewModelClasses;

        public static PartViewModel Create(Part part)
        {
            PopulateCache();

            var type = (s_partViewModelClasses.First(t => t.ConstructorType == part.GetType()));
            return (PartViewModel)Activator.CreateInstance(type.Type, part);
        }

        public static IEnumerable<PartViewModel> Create<T>() where T : Part
        {
            PopulateCache();

            return s_partViewModelClasses.Where(info =>
                (typeof(T).IsAssignableFrom(info.ConstructorType))).Select(p => Create(p));
        }

        private static PartViewModel Create(TypeInfo info)
        {
            return (PartViewModel)Activator.CreateInstance(info.Type, args: (Activator.CreateInstance(info.ConstructorType)));
        }

        private static void PopulateCache()
        {
            if (s_partViewModelClasses == null)
            {
                s_partViewModelClasses = typeof(PartViewModel).Assembly.GetTypes().Where(t =>
                    typeof(PartViewModel).IsAssignableFrom(t)).Select(t =>
                        new TypeInfo
                        {
                            Type = t,
                            ConstructorType = t.GetConstructors()[0].GetParameters()[0].ParameterType,
                        }).ToList();
            }
        }
    }
}
