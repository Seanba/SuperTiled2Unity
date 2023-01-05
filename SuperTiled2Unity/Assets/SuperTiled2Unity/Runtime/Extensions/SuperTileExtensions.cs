using UnityEngine;

namespace SuperTiled2Unity
{
    public static class SuperTileExtensions
    {
        public static bool TryGetProperty(this SuperTile tile, string propName, out CustomProperty property)
        {
            property = null;

            if (tile == null)
            {
                return false;
            }

            if (tile.m_CustomProperties == null)
            {
                return false;
            }

            if (tile.m_CustomProperties.TryGetProperty(propName, out property))
            {
                return true;
            }

            return false;
        }

        public static string GetPropertyValueAsString(this SuperTile tile, string propName)
        {
            return GetPropertyValueAsString(tile, propName, string.Empty);
        }

        public static string GetPropertyValueAsString(this SuperTile tile, string propName, string defaultValue)
        {
            CustomProperty property;
            if (TryGetProperty(tile, propName, out property))
            {
                return property.GetValueAsString();
            }

            return defaultValue;
        }

        public static bool GetPropertyValueAsBool(this SuperTile tile, string propName)
        {
            return GetPropertyValueAsBool(tile, propName, false);
        }

        public static bool GetPropertyValueAsBool(this SuperTile tile, string propName, bool defaultValue)
        {
            CustomProperty property;
            if (TryGetProperty(tile, propName, out property))
            {
                return property.GetValueAsBool();
            }

            return defaultValue;
        }

        public static int GetPropertyValueAsInt(this SuperTile tile, string propName)
        {
            return GetPropertyValueAsInt(tile, propName, 0);
        }

        public static int GetPropertyValueAsInt(this SuperTile tile, string propName, int defaultValue)
        {
            CustomProperty property;
            if (TryGetProperty(tile, propName, out property))
            {
                return property.GetValueAsInt();
            }

            return defaultValue;
        }

        public static float GetPropertyValueAsFloat(this SuperTile tile, string propName)
        {
            return GetPropertyValueAsFloat(tile, propName, 0);
        }

        public static float GetPropertyValueAsFloat(this SuperTile tile, string propName, float defaultValue)
        {
            CustomProperty property;
            if (TryGetProperty(tile, propName, out property))
            {
                return property.GetValueAsFloat();
            }

            return defaultValue;
        }

        public static Color GetPropertyValueAsColor(this SuperTile tile, string propName)
        {
            return GetPropertyValueAsColor(tile, propName, Color.clear);
        }

        public static Color GetPropertyValueAsColor(this SuperTile tile, string propName, Color defaultValue)
        {
            CustomProperty property;
            if (TryGetProperty(tile, propName, out property))
            {
                return property.GetValueAsColor();
            }

            return defaultValue;
        }

        public static T GetPropertyValueAsEnum<T>(this SuperTile tile, string propName)
        {
            return GetPropertyValueAsEnum(tile, propName, default(T));
        }

        public static T GetPropertyValueAsEnum<T>(this SuperTile tile, string propName, T defaultValue)
        {
            CustomProperty property;
            if (TryGetProperty(tile, propName, out property))
            {
                return property.GetValueAsEnum<T>();
            }

            return defaultValue;
        }
    }
}
