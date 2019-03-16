using System;
using System.Globalization;
using UnityEngine;

namespace SuperTiled2Unity
{
    public static class StringExtensions
    {
        public static Color ToColor(this string htmlString)
        {
            string html = htmlString;
            if (html.StartsWith("#"))
            {
                html = html.Remove(0, 1);
            }

            if (html.Length == 8)
            {
                // ARBG
                byte a = byte.Parse(html.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                byte r = byte.Parse(html.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(html.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(html.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, a);
            }
            else if (html.Length == 6)
            {
                // RBA
                byte r = byte.Parse(html.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(html.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(html.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, 255);
            }

            Debug.LogErrorFormat("Could not convert '{0}' to a color.", htmlString);
            return Color.magenta;
        }

        public static T ToEnum<T>(this string str)
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException(string.Format("Type '{0}' is not an enum.", typeof(T)));
            }

            var enumString = str.Replace("-", "_");
            T value;

            try
            {
                value = (T)Enum.Parse(typeof(T), enumString, true);
            }
            catch
            {
                Debug.LogErrorFormat("Could not convert '{0}' to enum type '{1}'.", enumString, typeof(T).Name);
                value = default(T);
            }

            return value;
        }

        public static float ToFloat(this string str)
        {
            float result;
            if (!float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                Debug.LogErrorFormat("Could not convert '{0}' to float.", str);
            }

            return result;
        }

        public static int ToInt(this string str)
        {
            int result;
            if (!int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                Debug.LogErrorFormat("Could not convert '{0}' to int.", str);
            }

            return result;
        }

        public static bool ToBool(this string str)
        {
            if (str.Equals("1") || str.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if (str.Equals("0") || str.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Debug.LogErrorFormat("Could not convert '{0}' to bool.", str);
            return false;
        }
    }
}
