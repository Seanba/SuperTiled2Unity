using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
            var baseType = typeof(CustomTmxImporter);
            var importers = from t in Assembly.GetAssembly(baseType).GetTypes()
                            where baseType.IsAssignableFrom(t)
                            where !t.IsAbstract
                            from attr in GetCustomAttributes(t, typeof(AutoCustomTmxImporterAttribute))
                            let auto = attr as AutoCustomTmxImporterAttribute
                            orderby auto.Order
                            select t;

            return importers.ToList();
        }
    }
}
