using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public static class CustomPropertyLoader
    {
        public static List<CustomProperty> LoadCustomPropertyListWithExpansion(XElement xProperties, SuperImportContext context)
        {
            List<CustomProperty> properties = new List<CustomProperty>();
            if (xProperties != null)
            {
                foreach (var xProperty in xProperties.Elements("property"))
                {
                    var property = LoadCustomPropertyWithExpansion(xProperty, context);
                    property.m_Value = property.m_JObject.ToString(Formatting.None);

                    if (!property.IsEmpty)
                    {
                        properties.Add(property);
                    }
                }
            }

            return properties;
        }

        private static CustomProperty LoadCustomPropertyWithExpansion(XElement xProperty, SuperImportContext context)
        {
            CustomProperty property = new CustomProperty();

            property.m_Name = xProperty.GetAttributeAs("name", "");
            var type = xProperty.GetAttributeAs("type", "string");

            if (type == "class")
            {
                property.m_Type = xProperty.GetAttributeAs("propertytype", "");
                Assert.IsFalse(property.m_Type.IsEmpty());
            }
            else
            {
                property.m_Type = type;
            }
            
            property.m_JObject = ParseXmlDescPropertiesToJObjectWithExpansion(
                xProperty, context, type == "class", property.m_Type);
            
            Assert.IsNotNull(property.m_JObject);

            return property;
        }
        
        private static JObject ParseXmlDescPropertiesToJObjectWithExpansion(
            XElement xProperty, SuperImportContext context, bool isCustomClass, string propertyType)
        {

            var jObj = new JObject();
            
            Assert.IsFalse(isCustomClass && propertyType.IsEmpty());
            jObj["type"] = isCustomClass ? "class" : propertyType;

            if (isCustomClass)
            {
                var realType = xProperty.GetAttributeAs("propertytype", "");
                Assert.IsFalse(realType.IsEmpty());
                jObj["realType"] = realType;

                var properties = xProperty.Element("properties");
                
                // Get default
                if (context.Settings.CustomObjectTypes.TryGetCustomObjectType(realType, out var predefined))
                {
                    foreach (var predefinedMCustomProperty in predefined.m_CustomProperties)
                    {
                        jObj[predefinedMCustomProperty.m_Name] =
                            GetExpandedJObject(predefinedMCustomProperty.m_JObject, context);   
                    }
                }
                
                // replace default
                if (properties is { })
                {
                    foreach (var xSubProperty in properties.Elements("property"))
                    {
                        var name = xSubProperty.GetAttributeAs<String>("name");
                        var isSubCustomClass = jObj[name]!["type"]!.ToString() == "class";
                        var subRealType = jObj[name]!["realType"]!.ToString();

                        jObj[name] = ParseXmlDescPropertiesToJObjectWithExpansion(
                            xSubProperty, context, isSubCustomClass, subRealType);
                    }
                }
            }
            else
            {
                GetPlaintValueFromXmlEelment(xProperty, jObj);
            }
            
            return jObj;
        }

        private static JObject GetExpandedJObject(JObject root, SuperImportContext context)
        {
            var expanded = new JObject();

            var type = root["type"].ToString();

            if (type == "class")
            {
                var realType = root["realType"].ToString();

                var sub = new JObject();
                // Get default
                if (context.Settings.CustomObjectTypes.TryGetCustomObjectType(realType, out var predefined))
                {
                    foreach (var predefinedMCustomProperty in predefined.m_CustomProperties)
                    {
                        sub[predefinedMCustomProperty.m_Name] =
                            GetExpandedJObject(predefinedMCustomProperty.m_JObject, context);   
                    }
                }

                expanded["realType"] = realType;
                expanded["type"] = "class";
                expanded["value"] = sub;
            }
            else
            {
                expanded["type"] = type;
                expanded["realType"] = type; 
                expanded["value"] = root["value"];
            }
            
            return expanded;
        }
        
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
            property.m_JObject = ParseXmlDescPropertiesToJObject(xProperty);

            Assert.IsNotNull(property.m_JObject);

            return property;
        }

        public static JObject ParseXmlDescPropertiesToJObject(XElement xProperty)
        {
            var propType = xProperty.GetAttributeAs("type", "");

            var jobj = new JObject();

            Assert.IsFalse(propType.IsEmpty());
            jobj["type"] = propType;

            if (propType == "class")
            {
                var realType = xProperty.GetAttributeAs("propertytype", "");
                Assert.IsFalse(realType.IsEmpty());
                jobj["realType"] = realType;
            }
            else
            {
                GetPlaintValueFromXmlEelment(xProperty, jobj);
            }
            
            return jobj;
        }

        private static void GetPlaintValueFromXmlEelment(XElement xProperty, JObject jobj)
        {
            var defaultVal = xProperty.GetAttributeAs<String>("default");
            if (defaultVal != null)
            {
                jobj["value"] = defaultVal;
            }
            else
            {
                var value = xProperty.GetAttributeAs<String>("value", "");
                Assert.IsFalse(value.IsEmpty());
                jobj["value"] = value;
            }
        }
    }
}