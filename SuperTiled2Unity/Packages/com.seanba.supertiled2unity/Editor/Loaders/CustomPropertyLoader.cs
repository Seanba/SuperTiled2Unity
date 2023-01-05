using System.Collections.Generic;
using System.Xml.Linq;

namespace SuperTiled2Unity.Editor
{
    public static class CustomPropertyLoader
    {
        public static List<CustomProperty> LoadCustomPropertyList(XElement xProperties)
        {
            List<CustomProperty> properties = new List<CustomProperty>();

            if (xProperties != null)
            {
                foreach (var xProperty in xProperties.Elements("property"))
                {
                    var property = LoadCustomProperty(xProperty);

                    if (!property.IsEmpty)
                    {
                        properties.Add(property);
                    }
                }
            }

            return properties;
        }

        public static CustomProperty LoadCustomProperty(XElement xProperty)
        {
            CustomProperty property = new CustomProperty();

            property.m_Name = xProperty.GetAttributeAs("name", "");
            property.m_Type = xProperty.GetAttributeAs("type", "string");

            // In some cases, value may be in the default attribute
            property.m_Value = xProperty.GetAttributeAs("default", "");

            // A value attribute overrides a default attribute
            if (!string.IsNullOrEmpty(xProperty.Value))
            {
                // Using inner text
                property.m_Value = xProperty.Value;
            }
            else
            {
                // Using value attribute
                property.m_Value = xProperty.GetAttributeAs<string>("value", property.m_Value);
            }

            return property;
        }
    }
}
