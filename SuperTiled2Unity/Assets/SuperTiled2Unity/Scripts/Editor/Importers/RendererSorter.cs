using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // Helper class for sorting all our TilemapRenderer and SpriteRenderer instances
    public class RendererSorter
    {
        private const string DefaultSortLayerName = "Default";

        private string m_CurrentSortLayerName = DefaultSortLayerName;
        private int m_CurrentSortOrder = 0;
        private int m_InsideGroupCounter = 0;

        public SortingMode SortingMode { get; set; }

        public void BeginTileLayer(SuperTileLayer layer)
        {
            if (m_InsideGroupCounter == 0)
            {
                SortingLayerCheck(layer.gameObject);
            }
        }

        public void EndTileLayer(SuperTileLayer layer)
        {
            if (SortingMode == SortingMode.CustomSortAxis)
            {
                m_CurrentSortOrder++;
            }
        }

        public void BeginObjectLayer(SuperObjectLayer layer)
        {
            if (m_InsideGroupCounter == 0)
            {
                SortingLayerCheck(layer.gameObject);
            }
        }

        public void EndObjectLayer(SuperObjectLayer layer)
        {
            if (SortingMode == SortingMode.CustomSortAxis)
            {
                m_CurrentSortOrder++;
            }
        }

        public void BeginGroupLayer(SuperGroupLayer layer) // fixit - figure out grouping when non-grouped custom axis is working
        {
            m_InsideGroupCounter++;
            SortingLayerCheck(layer.gameObject);
        }

        public void EndGroupLayer()
        {
            m_InsideGroupCounter--;

            // fixit - modify sort order? Under which conditions?
        }

        public string AssignSort(Renderer renderer)
        {
            var go = renderer.gameObject;

            SortingLayerCheck(go);
            renderer.sortingLayerName = m_CurrentSortLayerName;
            renderer.sortingOrder = m_CurrentSortOrder;

            // fixit - not sure about the logic
            if (m_InsideGroupCounter == 0 && SortingMode != SortingMode.CustomSortAxis)
            {
                m_CurrentSortOrder++;
            }

            return m_CurrentSortLayerName;
        }

        private void SortingLayerCheck(GameObject go)
        {
            // The game object may have custom properties that change how we sort
            CustomProperty property;
            if (go.TryGetCustomPropertySafe("unity:SortingLayer", out property) || go.TryGetCustomPropertySafe("unity:sortingLayerName", out property))
            {
                // Reset order on a new sorting layer
                var name = property.GetValueAsString();
                if (!string.Equals(name, m_CurrentSortLayerName, StringComparison.OrdinalIgnoreCase))
                {
                    m_CurrentSortLayerName = name;
                    m_CurrentSortOrder = 0;
                }
            }
        }
    }
}
