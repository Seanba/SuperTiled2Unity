using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class TiledCustomTypesJson
    {
        public static List<CustomObjectType> LoadCustomObjectTypes(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new List<CustomObjectType>();
            }

            try
            {
                object root = JsonParser.Parse(json);
                var propertyTypes = GetPropertyTypes(root);
                if (propertyTypes == null)
                {
                    return new List<CustomObjectType>();
                }

                return propertyTypes
                    .OfType<Dictionary<string, object>>()
                    .Where(IsClassType)
                    .Select(ToCustomObjectType)
                    .Where(t => !string.IsNullOrEmpty(t.m_Name))
                    .ToList();
            }
            catch (FormatException)
            {
                return new List<CustomObjectType>();
            }
        }

        public static List<CustomEnumType> LoadCustomEnumTypes(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new List<CustomEnumType>();
            }

            try
            {
                object root = JsonParser.Parse(json);
                var propertyTypes = GetPropertyTypes(root);
                if (propertyTypes == null)
                {
                    return new List<CustomEnumType>();
                }

                return propertyTypes
                    .OfType<Dictionary<string, object>>()
                    .Where(IsEnumType)
                    .Select(ToCustomEnumType)
                    .Where(t => !string.IsNullOrEmpty(t.m_Name))
                    .ToList();
            }
            catch (FormatException)
            {
                return new List<CustomEnumType>();
            }
        }

        public static T GetClassPropertyAs<T>(
            string className,
            string propertyName,
            T defaultValue,
            List<CustomObjectType> objectTypes) where T : IConvertible
        {
            CustomObjectType customObjectType;
            CustomProperty property;
            if (objectTypes.TryGetCustomObjectType(className, out customObjectType) &&
                customObjectType.m_CustomProperties.TryGetProperty(propertyName, out property))
            {
                return ConvertPropertyValue(property.m_Value, defaultValue);
            }

            return defaultValue;
        }

        public static void MergeInto(List<CustomObjectType> destination, List<CustomObjectType> source)
        {
            if (destination == null || source == null)
            {
                return;
            }

            foreach (var sourceType in source)
            {
                if (destination.TryGetCustomObjectType(sourceType.m_Name, out var existingType))
                {
                    existingType.m_CustomProperties.CombineFromSource(sourceType.m_CustomProperties);
                }
                else
                {
                    destination.Add(sourceType);
                }
            }
        }

        private static List<object> GetPropertyTypes(object root)
        {
            var exportedTypes = root as List<object>;
            if (exportedTypes != null)
            {
                return exportedTypes;
            }

            var wrapped = root as Dictionary<string, object>;
            if (wrapped != null)
            {
                object propertyTypes;
                if (wrapped.TryGetValue("propertyTypes", out propertyTypes))
                {
                    return propertyTypes as List<object>;
                }
            }

            return null;
        }

        private static bool IsClassType(Dictionary<string, object> propertyType)
        {
            return string.Equals(GetString(propertyType, "type"), "class", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsEnumType(Dictionary<string, object> propertyType)
        {
            return string.Equals(GetString(propertyType, "type"), "enum", StringComparison.OrdinalIgnoreCase);
        }

        private static CustomObjectType ToCustomObjectType(Dictionary<string, object> propertyType)
        {
            return new CustomObjectType
            {
                m_Name = GetString(propertyType, "name"),
                m_Color = ParseColor(GetString(propertyType, "color")),
                m_CustomProperties = LoadMembers(GetList(propertyType, "members"))
            };
        }

        private static CustomEnumType ToCustomEnumType(Dictionary<string, object> propertyType)
        {
            var storageType = GetString(propertyType, "storageType");
            return new CustomEnumType
            {
                m_Name = GetString(propertyType, "name"),
                m_StorageType = string.IsNullOrEmpty(storageType) ? "string" : storageType,
                m_Values = LoadEnumValues(GetList(propertyType, "values")),
                m_ValuesAsFlags = GetBool(propertyType, "valuesAsFlags")
            };
        }

        private static Color ParseColor(string htmlColor)
        {
            if (!string.IsNullOrEmpty(htmlColor))
            {
                try
                {
                    return htmlColor.ToColor();
                }
                catch (FormatException)
                {
                    return Color.gray;
                }
            }

            return Color.gray;
        }

        private static List<CustomProperty> LoadMembers(List<object> members)
        {
            if (members == null)
            {
                return new List<CustomProperty>();
            }

            return members
                .OfType<Dictionary<string, object>>()
                .Select(ToCustomProperty)
                .Where(p => !p.IsEmpty)
                .ToList();
        }

        private static CustomProperty ToCustomProperty(Dictionary<string, object> member)
        {
            var typeName = GetString(member, "type");

            return new CustomProperty
            {
                m_Name = GetString(member, "name"),
                m_Type = string.IsNullOrEmpty(typeName) ? "string" : typeName,
                m_PropertyType = GetString(member, "propertyType"),
                m_Value = NormalizeValue(GetValue(member, "value"))
            };
        }

        private static List<string> LoadEnumValues(List<object> values)
        {
            if (values == null)
            {
                return new List<string>();
            }

            return values.Select(NormalizeValue).ToList();
        }

        private static object GetValue(Dictionary<string, object> data, string key)
        {
            object value;
            return data.TryGetValue(key, out value) ? value : null;
        }

        private static string GetString(Dictionary<string, object> data, string key)
        {
            var value = GetValue(data, key);
            return value as string ?? string.Empty;
        }

        private static List<object> GetList(Dictionary<string, object> data, string key)
        {
            return GetValue(data, key) as List<object>;
        }

        private static bool GetBool(Dictionary<string, object> data, string key)
        {
            var value = GetValue(data, key);
            if (value is bool)
            {
                return (bool)value;
            }

            var text = value as string;
            return text == "1" || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeValue(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var text = value as string;
            if (text != null)
            {
                return text;
            }

            if (value is bool)
            {
                return (bool)value ? "true" : "false";
            }

            if (value is IConvertible)
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            return JsonWriter.Write(value);
        }

        private static T ConvertPropertyValue<T>(string value, T defaultValue) where T : IConvertible
        {
            if (typeof(T).IsEnum)
            {
                var enumValue = value?.Replace("-", "_");
                if (Enum.TryParse(typeof(T), enumValue, true, out var parsedValue))
                {
                    return (T)parsedValue;
                }

                return defaultValue;
            }

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

            try
            {
                return (T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaultValue;
            }
        }

        private class JsonParser
        {
            private readonly string m_Text;
            private int m_Index;

            private JsonParser(string text)
            {
                m_Text = text;
            }

            public static object Parse(string text)
            {
                var parser = new JsonParser(text);
                var value = parser.ParseValue();
                parser.SkipWhitespace();
                if (!parser.IsEnd)
                {
                    throw new FormatException("Unexpected characters after JSON value.");
                }

                return value;
            }

            private bool IsEnd => m_Index >= m_Text.Length;

            private char Current => IsEnd ? '\0' : m_Text[m_Index];

            private object ParseValue()
            {
                SkipWhitespace();

                if (IsEnd)
                {
                    throw new FormatException("Unexpected end of JSON.");
                }

                switch (Current)
                {
                    case '{':
                        return ParseObject();
                    case '[':
                        return ParseArray();
                    case '"':
                        return ParseString();
                    case 't':
                        ReadLiteral("true");
                        return true;
                    case 'f':
                        ReadLiteral("false");
                        return false;
                    case 'n':
                        ReadLiteral("null");
                        return null;
                    default:
                        return ParseNumber();
                }
            }

            private Dictionary<string, object> ParseObject()
            {
                var obj = new Dictionary<string, object>();
                Read('{');
                SkipWhitespace();

                if (TryRead('}'))
                {
                    return obj;
                }

                while (true)
                {
                    SkipWhitespace();
                    if (Current != '"')
                    {
                        throw new FormatException("Expected JSON object key.");
                    }

                    string key = ParseString();
                    SkipWhitespace();
                    Read(':');
                    obj[key] = ParseValue();
                    SkipWhitespace();

                    if (TryRead('}'))
                    {
                        return obj;
                    }

                    Read(',');
                }
            }

            private List<object> ParseArray()
            {
                var list = new List<object>();
                Read('[');
                SkipWhitespace();

                if (TryRead(']'))
                {
                    return list;
                }

                while (true)
                {
                    list.Add(ParseValue());
                    SkipWhitespace();

                    if (TryRead(']'))
                    {
                        return list;
                    }

                    Read(',');
                }
            }

            private string ParseString()
            {
                var builder = new StringBuilder();
                Read('"');

                while (!IsEnd)
                {
                    char c = m_Text[m_Index++];
                    if (c == '"')
                    {
                        return builder.ToString();
                    }

                    if (c != '\\')
                    {
                        builder.Append(c);
                        continue;
                    }

                    if (IsEnd)
                    {
                        throw new FormatException("Unfinished JSON string escape.");
                    }

                    char escaped = m_Text[m_Index++];
                    switch (escaped)
                    {
                        case '"':
                        case '\\':
                        case '/':
                            builder.Append(escaped);
                            break;
                        case 'b':
                            builder.Append('\b');
                            break;
                        case 'f':
                            builder.Append('\f');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        case 'u':
                            builder.Append(ParseUnicodeEscape());
                            break;
                        default:
                            throw new FormatException("Invalid JSON string escape.");
                    }
                }

                throw new FormatException("Unterminated JSON string.");
            }

            private char ParseUnicodeEscape()
            {
                if (m_Index + 4 > m_Text.Length)
                {
                    throw new FormatException("Invalid JSON unicode escape.");
                }

                string hex = m_Text.Substring(m_Index, 4);
                m_Index += 4;
                return (char)int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            private object ParseNumber()
            {
                int start = m_Index;

                if (Current == '-')
                {
                    m_Index++;
                }

                while (char.IsDigit(Current))
                {
                    m_Index++;
                }

                bool isFloat = false;
                if (Current == '.')
                {
                    isFloat = true;
                    m_Index++;
                    while (char.IsDigit(Current))
                    {
                        m_Index++;
                    }
                }

                if (Current == 'e' || Current == 'E')
                {
                    isFloat = true;
                    m_Index++;
                    if (Current == '+' || Current == '-')
                    {
                        m_Index++;
                    }

                    while (char.IsDigit(Current))
                    {
                        m_Index++;
                    }
                }

                if (start == m_Index)
                {
                    throw new FormatException("Expected JSON value.");
                }

                string number = m_Text.Substring(start, m_Index - start);
                if (isFloat)
                {
                    double parsedDouble;
                    if (double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedDouble))
                    {
                        return parsedDouble;
                    }
                }
                else
                {
                    long parsedLong;
                    if (long.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedLong))
                    {
                        return parsedLong;
                    }
                }

                throw new FormatException("Invalid JSON number.");
            }

            private void ReadLiteral(string literal)
            {
                for (int i = 0; i < literal.Length; i++)
                {
                    Read(literal[i]);
                }
            }

            private void Read(char expected)
            {
                if (IsEnd || m_Text[m_Index] != expected)
                {
                    throw new FormatException(string.Format("Expected '{0}'.", expected));
                }

                m_Index++;
            }

            private bool TryRead(char expected)
            {
                if (!IsEnd && m_Text[m_Index] == expected)
                {
                    m_Index++;
                    return true;
                }

                return false;
            }

            private void SkipWhitespace()
            {
                while (!IsEnd && char.IsWhiteSpace(Current))
                {
                    m_Index++;
                }
            }
        }

        private static class JsonWriter
        {
            public static string Write(object value)
            {
                var builder = new StringBuilder();
                WriteValue(builder, value);
                return builder.ToString();
            }

            private static void WriteValue(StringBuilder builder, object value)
            {
                if (value == null)
                {
                    builder.Append("null");
                    return;
                }

                var text = value as string;
                if (text != null)
                {
                    WriteString(builder, text);
                    return;
                }

                if (value is bool)
                {
                    builder.Append((bool)value ? "true" : "false");
                    return;
                }

                var list = value as List<object>;
                if (list != null)
                {
                    WriteArray(builder, list);
                    return;
                }

                var obj = value as Dictionary<string, object>;
                if (obj != null)
                {
                    WriteObject(builder, obj);
                    return;
                }

                builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
            }

            private static void WriteArray(StringBuilder builder, List<object> list)
            {
                builder.Append('[');
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(',');
                    }

                    WriteValue(builder, list[i]);
                }

                builder.Append(']');
            }

            private static void WriteObject(StringBuilder builder, Dictionary<string, object> obj)
            {
                builder.Append('{');
                bool needsComma = false;
                foreach (var pair in obj)
                {
                    if (needsComma)
                    {
                        builder.Append(',');
                    }

                    WriteString(builder, pair.Key);
                    builder.Append(':');
                    WriteValue(builder, pair.Value);
                    needsComma = true;
                }

                builder.Append('}');
            }

            private static void WriteString(StringBuilder builder, string text)
            {
                builder.Append('"');
                foreach (char c in text)
                {
                    switch (c)
                    {
                        case '"':
                            builder.Append("\\\"");
                            break;
                        case '\\':
                            builder.Append("\\\\");
                            break;
                        case '\b':
                            builder.Append("\\b");
                            break;
                        case '\f':
                            builder.Append("\\f");
                            break;
                        case '\n':
                            builder.Append("\\n");
                            break;
                        case '\r':
                            builder.Append("\\r");
                            break;
                        case '\t':
                            builder.Append("\\t");
                            break;
                        default:
                            builder.Append(c);
                            break;
                    }
                }

                builder.Append('"');
            }
        }
    }
}
