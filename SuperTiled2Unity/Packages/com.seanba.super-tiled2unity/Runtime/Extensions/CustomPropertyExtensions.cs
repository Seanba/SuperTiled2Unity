using UnityEngine;

namespace SuperTiled2Unity
{
    public static class CustomPropertyExtensions
    {
        public static string GetValueAsString(this CustomProperty property)
        {
            return property.m_Value;
        }

        public static Color GetValueAsColor(this CustomProperty property)
        {
            return property.m_Value.ToColor();
        }

        public static int GetValueAsInt(this CustomProperty property)
        {
            return property.m_Value.ToInt();
        }

        public static float GetValueAsFloat(this CustomProperty property)
        {
            return property.m_Value.ToFloat();
        }

        public static bool GetValueAsBool(this CustomProperty property)
        {
            return property.m_Value.ToBool();
        }

        public static T GetValueAsEnum<T>(this CustomProperty property)
        {
            return property.m_Value.ToEnum<T>();
        }
    }
}
