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

        public virtual CustomProperty GetCustomProperty(string key) => this;

        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(m_Name); }
        }
    }

    [Serializable]
    public class ClassCustomProperty : CustomProperty
    {
        public Dictionary<string, CustomProperty> m_CustomProperties = new Dictionary<string, CustomProperty>();

        public override CustomProperty GetCustomProperty(string key)
        {
            if (m_CustomProperties.TryGetValue(key, out var customProperty))
                return customProperty;
            return null;
        }

    }

    // Helper extension methods
    public static class CustomPropertyListExtensions
    {
        public static bool TryGetProperty(this List<CustomProperty> list, string propertyName, out CustomProperty property)
        {
            if (list != null)
            {
                property = list.Find(p => String.Equals(p.GetCustomProperty(propertyName)?.m_Name, propertyName, StringComparison.OrdinalIgnoreCase));
                if (property != null)
                {
                    property = property.GetCustomProperty(propertyName);
                    return true;
                }
                return false;
            }

            property = null;
            return false;
        }
    }
}