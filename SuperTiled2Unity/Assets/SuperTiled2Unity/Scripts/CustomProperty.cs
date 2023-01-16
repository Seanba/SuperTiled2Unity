using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

namespace SuperTiled2Unity
{
    [Serializable]
    public class CustomProperty
    {
        private static Dictionary<string, JObject> JObjectDictionary_ = new Dictionary<string, JObject>();

        public string m_Name;
        public string m_Type;
        public string m_Value;

        public JObject m_JObject
        {
            get => JObjectDictionary_[m_Name];
            set => JObjectDictionary_[m_Name] = value;
        }

        public CustomProperty CloneProperty()
        {
            var newObject = new CustomProperty();
            newObject.m_Name = m_Name;
            newObject.m_Type = m_Type;
            newObject.m_JObject = m_JObject;

            return newObject;
        }
        
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(m_Name) && m_JObject != null; }
        }
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
