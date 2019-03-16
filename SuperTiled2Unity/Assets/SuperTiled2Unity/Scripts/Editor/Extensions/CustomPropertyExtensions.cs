using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class CustomPropertyExtensions
    {
        public static CustomProperty CloneProperty(this CustomProperty property)
        {
            var cloned = new CustomProperty();
            cloned.m_Name = property.m_Name;
            cloned.m_Type = property.m_Type;
            cloned.m_Value = property.m_Value;

            return cloned;
        }

        public static int CombineFromSource(this List<CustomProperty> list, List<CustomProperty> source)
        {
            int numAdded = 0;

            if (source != null)
            {
                foreach (var typeProp in source)
                {
                    // Do not add properties if they already exist
                    if (!list.Any(p => string.Equals(p.m_Name, typeProp.m_Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        list.Add(typeProp.CloneProperty());
                        numAdded++;
                    }
                }
            }

            return numAdded;
        }

        public static int AddPropertiesFromType(this List<CustomProperty> list, string typeName, SuperImportContext importContext)
        {
            if (list == null)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(typeName))
            {
                return 0;
            }

            CustomObjectType objectType;
            if (importContext.Settings.CustomObjectTypes.TryGetCustomObjectType(typeName, out objectType))
            {
                return CombineFromSource(list, objectType.m_CustomProperties);
            }

            return 0;
        }
    }
}
