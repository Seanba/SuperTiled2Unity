using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperTiled2Unity.Editor
{
    public static class CustomPropertyExtensions
    {
        public static CustomProperty CloneProperty(this CustomProperty property)
        {
            var cloned = new CustomProperty();
            cloned.m_Name = property.m_Name;
            cloned.m_Type = property.m_Type;
            cloned.m_PropertyType = property.m_PropertyType;
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
            return list.AddPropertiesFromType(typeName, ST2USettings.instance.m_CustomObjectTypes);
        }

        public static int ResolveEnumStorageTypes(this List<CustomProperty> list, List<CustomEnumType> enumTypes)
        {
            if (list == null || enumTypes.IsEmpty())
            {
                return 0;
            }

            int numResolved = 0;
            foreach (var property in list)
            {
                if (string.IsNullOrEmpty(property.m_PropertyType))
                {
                    continue;
                }

                CustomEnumType enumType;
                if (!enumTypes.TryGetCustomEnumType(property.m_PropertyType, out enumType))
                {
                    continue;
                }

                var storageType = string.IsNullOrEmpty(enumType.m_StorageType) ? "string" : enumType.m_StorageType;
                if (!string.Equals(property.m_Type, storageType, StringComparison.OrdinalIgnoreCase))
                {
                    property.m_Type = storageType;
                    numResolved++;
                }
            }

            return numResolved;
        }

        public static int AddPropertiesFromType(this List<CustomProperty> list, string typeName, List<CustomObjectType> objectTypes)
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
            if (objectTypes.TryGetCustomObjectType(typeName, out objectType))
            {
                return CombineFromSource(list, objectType.m_CustomProperties);
            }

            return 0;
        }
    }
}
