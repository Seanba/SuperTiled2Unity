using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperTiled2Unity.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoCustomWorldImporterAttribute : Attribute
    {
        private static List<Type> m_CachedImporters;

        public AutoCustomWorldImporterAttribute()
        {
            Order = 0;
        }

        public AutoCustomWorldImporterAttribute(int order)
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

            var importers = from t in AppDomain.CurrentDomain.GetAllDerivedTypes<CustomWorldImporter>()
                            where !t.IsAbstract
                            from attr in GetCustomAttributes(t, typeof(AutoCustomWorldImporterAttribute))
                            let auto = attr as AutoCustomWorldImporterAttribute
                            orderby auto.Order
                            select t;

            m_CachedImporters = importers.ToList();
            return m_CachedImporters;
        }
    }
}
