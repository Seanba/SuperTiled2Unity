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
}
