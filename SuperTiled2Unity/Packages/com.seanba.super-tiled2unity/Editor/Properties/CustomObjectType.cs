using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    [Serializable]
    public class CustomObjectType
    {
        public string m_Name;
        public Color m_Color;
        public List<CustomProperty> m_CustomProperties;
    }

    [Serializable]
    public class CustomEnumType
    {
        public string m_Name;
        public string m_StorageType;
        public List<string> m_Values;
        public bool m_ValuesAsFlags;
    }

    public static class CustomObjectTypeExtensions
    {
        public static bool TryGetCustomObjectType(this List<CustomObjectType> list, string type, out CustomObjectType customObjectType)
        {
            if (list.IsEmpty())
            {
                customObjectType = null;
                return false;
            }

            customObjectType = list.FirstOrDefault(o => o.m_Name.Equals(type, StringComparison.OrdinalIgnoreCase));
            return customObjectType != null;
        }
    }

    public static class CustomEnumTypeExtensions
    {
        public static bool TryGetCustomEnumType(this List<CustomEnumType> list, string type, out CustomEnumType customEnumType)
        {
            if (list.IsEmpty())
            {
                customEnumType = null;
                return false;
            }

            customEnumType = list.FirstOrDefault(o => o.m_Name.Equals(type, StringComparison.OrdinalIgnoreCase));
            return customEnumType != null;
        }
    }
}
