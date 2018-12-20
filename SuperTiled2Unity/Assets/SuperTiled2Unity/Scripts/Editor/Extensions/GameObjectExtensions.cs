using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public static class GameObjectExtensions
    {
        public static float GetSuperPropertyValueFloat(this GameObject go, string propName, float defaultValue)
        {
            CustomProperty property;
            if (go.TryGetCustomPropertySafe(propName, out property))
            {
                return property.GetValueAsFloat();
            }

            return defaultValue;
        }

        public static int GetSuperPropertyValueInt(this GameObject go, string propName, int defaultValue)
        {
            CustomProperty property;
            if (go.TryGetCustomPropertySafe(propName, out property))
            {
                return property.GetValueAsInt();
            }

            return defaultValue;
        }

        public static bool GetSuperPropertyValueBool(this GameObject go, string propName, bool defaultValue)
        {
            CustomProperty property;
            if (go.TryGetCustomPropertySafe(propName, out property))
            {
                return property.GetValueAsBool();
            }

            return defaultValue;
        }

        public static Color GetSuperPropertyValueColor(this GameObject go, string propName, Color defaultValue)
        {
            CustomProperty property;
            if (go.TryGetCustomPropertySafe(propName, out property))
            {
                return property.GetValueAsColor();
            }

            return defaultValue;
        }

        public static T GetSuperPropertyValueEnum<T>(this GameObject go, string propName, T defaultValue)
        {
            CustomProperty property;
            if (go.TryGetCustomPropertySafe(propName, out property))
            {
                return property.GetValueAsEnum<T>();
            }

            return defaultValue;
        }

        public static string GetSuperPropertyValueString(this GameObject go, string propName, string defaultValue)
        {
            CustomProperty property;
            if (go.TryGetCustomPropertySafe(propName, out property))
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

            // Position the layer based on the x, y offsets and pixels per unit
            goLayer.transform.localPosition = importContext.MakePoint(layerComponent.m_OffsetX, layerComponent.m_OffsetY);

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
    }
}
