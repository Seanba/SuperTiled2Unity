using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class XmlExtensions
    {
        public static Vector2[] GetAttributeAsVector2Array(this XElement element, string name)
        {
            if (element == null)
            {
                return null;
            }

            var data = element.GetAttributeAs<string>(name);
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            var vectors = from v in data.Split(' ')
                          let a = v.Split(',').ToArray()
                          let x = Convert.ToSingle(a[0], CultureInfo.InvariantCulture)
                          let y = Convert.ToSingle(a[1], CultureInfo.InvariantCulture)
                          select new Vector2(x, y);
            return vectors.ToArray();
        }

        public static Color GetAttributeAsColor(this XElement element, string name, Color defaultColor)
        {
            XAttribute attr = element.Attribute(name);
            if (attr == null)
            {
                return defaultColor;
            }

            string htmlColor = element.GetAttributeAs<string>(name);

            // Sometimes Tiled saves out color without the leading # but we expect it to be there
            if (!htmlColor.StartsWith("#"))
            {
                htmlColor = "#" + htmlColor;
            }

            if (htmlColor.Length == 9)
            {
                // ARBG
                byte a = byte.Parse(htmlColor.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                byte r = byte.Parse(htmlColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(htmlColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(htmlColor.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, a);
            }
            else if (htmlColor.Length == 7)
            {
                // RBA
                byte r = byte.Parse(htmlColor.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(htmlColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(htmlColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, 255);
            }

            // If we're here then we've got a bad color format. Just return an ugly color.
            return Color.magenta;
        }

        public static T GetAttributeAs<T>(this XElement element, string name, T defaultValue) where T : IConvertible
        {
            if (element == null)
            {
                return defaultValue;
            }

            var attribute = element.Attribute(name);
            if (attribute == null)
            {
                return defaultValue;
            }

            string value = attribute.Value;

            // Special case for enum
            if (typeof(T).IsEnum)
            {
                return GetStringAsEnum<T>(value, defaultValue);
            }

            // Special case for bool
            if (typeof(T) == typeof(bool))
            {
                if (value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
                {
                    value = bool.TrueString;
                }
                else if (value == "0" || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                {
                    value = bool.FalseString;
                }
            }

            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public static T GetAttributeAs<T>(this XElement element, string name) where T : IConvertible
        {
            return element.GetAttributeAs<T>(name, default(T));
        }

        // Helper method for enums
        private static T GetStringAsEnum<T>(string enumStringValue, T defaultValue)
        {
            var enumString = enumStringValue.Replace("-", "_");

            T value = defaultValue;
            try
            {
                value = (T)Enum.Parse(typeof(T), enumString, true);
            }
            catch
            {
                Debug.LogErrorFormat("Could not convert string '{0}' to enum type '{1}'. Using default value '{2}'", enumStringValue, typeof(T).Name, defaultValue);
            }

            return value;
        }

        // Helper method to combine two elements
        public static void CombineWithTemplate(this XElement xObject, string template)
        {
            XElement xTemplate = XElement.Parse(template);

            // The template will add additional attributes to the xElement
            // Where there is a collision, the xObject wins outs
            foreach (var xa in xTemplate.Attributes())
            {
                var source = xObject.Attribute(xa.Name);
                if (source == null)
                {
                    // Root attribute brought over from the template
                    xObject.SetAttributeValue(xa.Name, xa.Value);
                }
            }

            // Combine child elements (But not properties yet. Those are a special case.)
            foreach (var xl in xTemplate.Elements())
            {
                if (xl.Name.LocalName != "properties")
                {
                    var source = xObject.Element(xl.Name);
                    if (source == null)
                    {
                        xObject.Add(xl);
                    }
                }
            }

            // Combine the properties from source and template
            var properties = new Dictionary<string, XElement>();

            // Collect the properties from the template first, by named key
            foreach (var prop in xTemplate.Descendants("property"))
            {
                properties.Add(prop.GetAttributeAs<string>("name"), prop);
            }

            // Collect the properties on the source, overriding by named key
            foreach (var prop in xObject.Descendants("property"))
            {
                properties.Add(prop.GetAttributeAs<string>("name"), prop);
            }

            // Put the combined properties into the object
            if (properties.Any())
            {
                // Remove the old properties
                var xRemove = xObject.Element("properties");
                if (xRemove != null)
                {
                    xRemove.Remove();
                }

                // Add the combined properties
                xObject.Add(new XElement("properties", properties.Values));
            }
        }
    }
}
