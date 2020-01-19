using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperTiled2Unity.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoCustomTmxImporterAttribute : Attribute
    {
        public AutoCustomTmxImporterAttribute()
        {
            Order = 0;
        }

        public AutoCustomTmxImporterAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; private set; }

        public static List<Type> GetOrderedAutoImportersTypes()
        {
            var importers = from t in AppDomain.CurrentDomain.GetAllDerivedTypes<CustomTmxImporter>()
                            where !t.IsAbstract
                            from attr in GetCustomAttributes(t, typeof(AutoCustomTmxImporterAttribute))
                            let auto = attr as AutoCustomTmxImporterAttribute
                            orderby auto.Order
                            select t;

            return importers.ToList();
        }
    }
}
