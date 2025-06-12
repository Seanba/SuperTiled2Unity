using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public static class GameObjectExtensions
    {
        public static float GetSuperPropertyValueFloat(this GameObject go, string propName, float defaultValue)
        {
            if (go.TryGetCustomPropertySafe(propName, out CustomProperty property))
            {
                return property.GetValueAsFloat();
            }

            return defaultValue;
        }

        public static int GetSuperPropertyValueInt(this GameObject go, string propName, int defaultValue)
        {
            if (go.TryGetCustomPropertySafe(propName, out CustomProperty property))
            {
                return property.GetValueAsInt();
            }

            return defaultValue;
        }

        public static bool GetSuperPropertyValueBool(this GameObject go, string propName, bool defaultValue)
        {
            if (go.TryGetCustomPropertySafe(propName, out CustomProperty property))
            {
                return property.GetValueAsBool();
            }

            return defaultValue;
        }

        public static Color GetSuperPropertyValueColor(this GameObject go, string propName, Color defaultValue)
        {
            if (go.TryGetCustomPropertySafe(propName, out CustomProperty property))
            {
                return property.GetValueAsColor();
            }

            return defaultValue;
        }

        public static T GetSuperPropertyValueEnum<T>(this GameObject go, string propName, T defaultValue)
        {
            if (go.TryGetCustomPropertySafe(propName, out CustomProperty property))
            {
                return property.GetValueAsEnum<T>();
            }

            return defaultValue;
        }

        public static string GetSuperPropertyValueString(this GameObject go, string propName, string defaultValue)
        {
            if (go.TryGetCustomPropertySafe(propName, out CustomProperty property))
            {
                return property.GetValueAsString();
            }

            return defaultValue;
        }

        public static void AddChildWithUniqueName(this GameObject go, GameObject child)
        {
            if (go == null)
            {
                return;
            }

            // Make sure the child name is unqiue
            string name = child.name;
            int count = 0;

            while (go.transform.Find(name) != null)
            {
                name = string.Format("{0} ({1})", child.name, ++count);
            }

            child.name = name;
            child.transform.SetParent(go.transform, false);
        }

        // Creates a new object, attached to the parent, with a specialized layer component
        public static T AddSuperLayerGameObject<T>(this GameObject goParent, SuperLayerLoader loader, SuperImportContext importContext) where T : SuperLayer
        {
            GameObject goLayer = new GameObject();

            // Read in the fields common across our Tiled layer types
            var layerComponent = loader.CreateLayer(goLayer) as T;
            Assert.IsNotNull(layerComponent);

            // Add the object to the parent
            goLayer.name = layerComponent.m_TiledName;
            goParent.AddChildWithUniqueName(goLayer);

            return layerComponent;
        }

        // Assing the layers on all children to our layer
        public static void AssignChildLayers(this GameObject goParent)
        {
            foreach (Transform child in goParent.transform)
            {
                child.ChangeLayersRecursively(goParent.layer);
            }
        }

        private static void ChangeLayersRecursively(this Transform trans, int layer)
        {
            trans.gameObject.layer = layer;
            foreach (Transform child in trans)
            {
                child.ChangeLayersRecursively(layer);
            }
        }

        public static void BroadcastProperty(this GameObject go, CustomProperty property, Dictionary<int, GameObject> objectsById, Action<string> ErrorCallback)
        {
            object objValue;

            if (property.m_Type == "bool")
            {
                objValue = property.GetValueAsBool();
            }
            else if (property.m_Type == "color")
            {
                objValue = property.GetValueAsColor();
            }
            else if (property.m_Type == "float")
            {
                objValue = property.GetValueAsFloat();
            }
            else if (property.m_Type == "int")
            {
                objValue = property.GetValueAsInt();
            }
            else if (property.m_Type == "object")
            {
                var objectId = property.GetValueAsInt();
                if (!objectsById.TryGetValue(objectId, out GameObject gameObject))
                {
                    ErrorCallback($"Object property refers to invalid ID {objectId}");
                    return;
                }
                else
                {
                    objValue = gameObject;
                }
            }
            else
            {
                objValue = property.GetValueAsString();
            }

            // Use properties on all types in hierary that inherit from MonoBehaviour
            var components = go.GetComponentsInChildren<MonoBehaviour>();
            foreach (var comp in components)
            {
                // Look for methods first
                var method = FindMethodBySignature(comp, property.m_Name, objValue.GetType());
                if (method != null)
                {
                    try
                    {
                        method.Invoke(comp, new object[1] { objValue });
                    }
                    catch (TargetInvocationException tie)
                    {
                        ErrorCallback($"Error invoking '{comp.GetType()}.{method.Name}({objValue})' method. Exception = '{tie.InnerException.Message}'");
                    }
                    catch (Exception e)
                    {
                        ErrorCallback($"Component method '{comp.GetType()}.{method.Name}({objValue})' threw an exception: '{e.Message}'");
                    }
                    continue;
                }

                // Then properties
                var csprop = FindPropertyBySignature(comp, property.m_Name, objValue.GetType());
                if (csprop != null)
                {
                    csprop.SetValue(comp, objValue, null);
                    continue;
                }

                // Finally, look for public fields
                var csfield = FindFieldBySignature(comp, property.m_Name, objValue.GetType());
                if (csfield != null)
                {
                    try
                    {
                        csfield.SetValue(comp, objValue);
                    }
                    catch (Exception e)
                    {
                        ErrorCallback($"Error setting '{comp.GetType()}.{csfield.Name} = {objValue}' method. Exception = '{e.Message}'");
                    }
                    continue;
                }
            }
        }

        private static MethodInfo FindMethodBySignature(MonoBehaviour component, string name, Type paramType)
        {
            return component.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(info =>
            {
                // Name must match
                if (info.Name != name)
                {
                    return false;
                }

                // Return type must be void
                if (info.ReturnType != typeof(void))
                {
                    return false;
                }

                // Must have one parameter that matches param type
                var parameters = info.GetParameters();
                if (parameters.Length != 1)
                {
                    return false;
                }

                // Parameter type must match (or be implicitly convertable)
                var pinfo = parameters[0];
                if (pinfo.ParameterType == paramType)
                {
                    // Exact match
                    return true;
                }

                if (pinfo.ParameterType.IsEnum && paramType == typeof(int))
                {
                    // ints are converted to enums automatically
                    return true;
                }

                return false;
            }).FirstOrDefault();
        }

        private static PropertyInfo FindPropertyBySignature(MonoBehaviour component, string name, Type valueType)
        {
            // Property must be public and instanced and writable
            return component.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(
                info =>
                info.CanWrite &&
                info.Name == name &&
                ((info.PropertyType == valueType) || (info.PropertyType.IsEnum && valueType == typeof(int)))
                ).FirstOrDefault();
        }

        private static FieldInfo FindFieldBySignature(MonoBehaviour component, string name, Type valueType)
        {
            return component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).Where(
                info =>
                !info.IsInitOnly &&
                info.Name == name &&
                ((info.FieldType == valueType) || (info.FieldType.IsEnum && valueType == typeof(int)))
                ).FirstOrDefault();
        }
    }
}
