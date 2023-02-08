using System;
using System.Collections.Generic;

namespace SuperTiled2Unity
{
    [Serializable]
    public class CustomProperty
    {
        public string m_Name;
        public string m_Type;
        public string m_Value;

        public bool IsEmpty => string.IsNullOrEmpty(m_Name);
    }

    // Helper extension methods
    public static class CustomPropertyListExtensions
    {
        public static bool TryGetProperty(this List<CustomProperty> list, string propertyName, out CustomProperty property)
        {
            if (list != null)
            {
                property = list.Find(p => String.Equals(p.m_Name, propertyName, StringComparison.OrdinalIgnoreCase));
                return property != null;
            }

            property = null;
            return false;
        }
    }
}
