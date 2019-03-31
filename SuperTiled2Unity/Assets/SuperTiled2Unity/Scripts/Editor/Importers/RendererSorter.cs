using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity.Editor
{
    // Helper class for sorting all our TilemapRenderer and SpriteRenderer instances
    public class RendererSorter
    {
        private const string DefaultSortLayerName = "Default";

        private string m_CurrentSortLayerName = DefaultSortLayerName;
        private int m_CurrentSortOrder = 0;

        public SortingMode SortingMode { get; set; }

        public void BeginTileLayer(SuperTileLayer layer)
        {
            SortingLayerCheck(layer.gameObject);
        }

        public void EndTileLayer(SuperTileLayer layer)
        {
        }

        public void BeginObjectLayer(SuperObjectLayer layer)
        {
            SortingLayerCheck(layer.gameObject);
        }

        public void EndObjectLayer(SuperObjectLayer layer)
        {
        }

        public void BeginGroupLayer(SuperGroupLayer layer)
        {
            //SortingLayerCheck(layer.gameObject); // fixit - get general case working first
        }

        public void EndGroupLayer()
        {
        }

        public string AssignTilemapSort(TilemapRenderer renderer)
        {
            var go = renderer.gameObject;

            SortingLayerCheck(go);
            renderer.sortingLayerName = m_CurrentSortLayerName;
            renderer.sortingOrder = m_CurrentSortOrder;

            // fixit - for grouping we will not advance
            m_CurrentSortOrder++;

            return m_CurrentSortLayerName;
        }

        public string AssignSpriteSort(SpriteRenderer renderer)
        {
            var go = renderer.gameObject;

            SortingLayerCheck(go);
            renderer.sortingLayerName = m_CurrentSortLayerName;
            renderer.sortingOrder = m_CurrentSortOrder;

            // Sprites will either have a specfic sort order or they will be sorted by a custom axis
            if (SortingMode == SortingMode.Stacked)
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
