using UnityEngine;

namespace SuperTiled2Unity
{
    public static class CustomPropertyExtensions
    {
        public static string GetValueAsString(this CustomProperty property)
        {
            return property.m_JObject["value"]!.ToString();
        }

        public static Color GetValueAsColor(this CustomProperty property)
        {
            return property.m_JObject["value"]!.ToString().ToColor();
        }

        public static int GetValueAsInt(this CustomProperty property)
        {
            return property.m_JObject["value"]!.ToString().ToInt();
        }

        public static float GetValueAsFloat(this CustomProperty property)
        {
            return property.m_JObject["value"]!.ToString().ToFloat();
        }

        public static bool GetValueAsBool(this CustomProperty property)
        {
            return property.m_JObject["value"]!.ToString().ToBool();
        }

        public static T GetValueAsEnum<T>(this CustomProperty property)
        {
            return property.m_JObject["value"]!.ToString().ToEnum<T>();
        }
    }
}
