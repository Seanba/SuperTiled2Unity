using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public class LayerSorterHelper
    {
        private const string DefaultLayerName = "Default";

        private Dictionary<string, int> m_SortingLayers;

        public LayerSorterHelper()
        {
            m_SortingLayers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var sort in SortingLayer.layers)
            {
                m_SortingLayers.Add(sort.name, 0);
            }

            Assert.IsTrue(m_SortingLayers.ContainsKey(DefaultLayerName));
        }

        public void SortNewLayer(SuperLayer layer)
        {
            // Determine sorting layer using our own data
            bool sorted = SortNewLayerFromSource(layer, layer);

            if (!sorted)
            {
                // Determine sorting layer using parent data
                sorted = SortNewLayerFromSource(layer.GetComponentInAncestor<SuperLayer>(), layer);
            }

            if (!sorted)
            {
                // Just sort with the default sorting layer
                SortWithName(layer, DefaultLayerName);
            }
        }

        private bool SortNewLayerFromSource(SuperLayer source, SuperLayer target)
        {
            if (source == null)
            {
                return false;
            }

            // Use an explicit property to set the sorting layer
            CustomProperty property;
            if (source.gameObject.TryGetCustomPropertySafe("unity:sortingLayerName", out property))
            {
                // When using an explicit custom property for sorting layer then we make sure the out property name is a recognized sorting layer
                if (!m_SortingLayers.ContainsKey(property.m_Value))
                {
                    m_SortingLayers.Add(property.m_Value, 0);
                }

                return SortWithName(target, property.m_Value);
            }

            // Test if there is a match based on source layer name
            if (m_SortingLayers.ContainsKey(source.m_TiledName))
            {
                return SortWithName(target, source.m_TiledName);
            }

            return false;
        }

        private bool SortWithName(SuperLayer layer, string name)
        {
            Assert.IsNotNull(layer);
            Assert.IsTrue(m_SortingLayers.ContainsKey(name));

            var order = m_SortingLayers[name];
            layer.m_SortingLayerName = name;
            layer.m_SortingOrder = order;

            // Increment for next use
            m_SortingLayers[name] = order + 1;

            return true;
        }
    }
}
