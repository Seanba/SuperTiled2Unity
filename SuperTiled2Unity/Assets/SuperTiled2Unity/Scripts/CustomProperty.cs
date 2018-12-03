using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity
{
    [Serializable]
    public class CustomProperty
    {
        public string m_Name;
        public string m_Type;
        public string m_Value;

        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(m_Name); }
        }
    }

    // Helper extension methods
    public static class CustomPropertyExtensions
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
