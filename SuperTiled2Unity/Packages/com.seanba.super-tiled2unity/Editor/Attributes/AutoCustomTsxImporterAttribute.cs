using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperTiled2Unity.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoCustomTsxImporterAttribute : Attribute
    {
        private static List<Type> m_CachedImporters;

        public AutoCustomTsxImporterAttribute()
        {
            Order = 0;
        }

        public AutoCustomTsxImporterAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; private set; }

        public static List<Type> GetOrderedAutoImportersTypes()
        {
            if (m_CachedImporters != null)
            {
                return m_CachedImporters;
            }

            var importers = from t in AppDomain.CurrentDomain.GetAllDerivedTypes<CustomTsxImporter>()
                            where !t.IsAbstract
                            from attr in GetCustomAttributes(t, typeof(AutoCustomTsxImporterAttribute))
                            let auto = attr as AutoCustomTsxImporterAttribute
                            orderby auto.Order
                            select t;

            m_CachedImporters = importers.ToList();
            return m_CachedImporters;
        }
    }
}
