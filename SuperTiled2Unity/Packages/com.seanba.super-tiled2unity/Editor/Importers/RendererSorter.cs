using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity.Editor
{
    // Helper class for sorting all our TilemapRenderer and SpriteRenderer instances
    public class RendererSorter
    {
        private const string DefaultSortLayerName = "Default";

        private string m_CurrentSortLayerName = DefaultSortLayerName;
        private int m_CurrentSortOrder;
        private int m_GroupDepth;

        public SortingMode SortingMode { get; set; }

        public int CurrentTileZ { get; private set; }

        public void BeginTileLayer(SuperTileLayer layer)
        {
            SortingLayerCheck(layer.gameObject);
        }

        public void EndTileLayer(SuperTileLayer layer)
        {
            if (IsInGroup())
            {
                CurrentTileZ++;
            }
            else
            {
                // Next tilemap will render on top of the this one
                m_CurrentSortOrder++;
            }
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
            CurrentTileZ = 0;
            m_GroupDepth++;
            SortingLayerCheck(layer.gameObject);
        }

        public void EndGroupLayer()
        {
            m_GroupDepth--;
            m_CurrentSortOrder++;
            CurrentTileZ = 0;
        }

        public string AssignTilemapSort(TilemapRenderer renderer)
        {
            var go = renderer.gameObject;

            SortingLayerCheck(go);
            renderer.sortingLayerName = m_CurrentSortLayerName;
            renderer.sortingOrder = m_CurrentSortOrder;

            return m_CurrentSortLayerName;
        }

        public string AssignSpriteSort(SpriteRenderer renderer)
        {
            var go = renderer.gameObject;

            SortingLayerCheck(go);
            renderer.sortingLayerName = m_CurrentSortLayerName;
            renderer.sortingOrder = m_CurrentSortOrder;

            // Sprites will either have a specfic sort order or they will be sorted by a custom axis or group
            if (SortingMode == SortingMode.Stacked && !IsInGroup())
            {
                m_CurrentSortOrder++;
            }

            return m_CurrentSortLayerName;
        }

        public bool IsUsingGroups()
        {
            // Grouping depends on custom sort axis sorting mode
            return SortingMode == SortingMode.CustomSortAxis;
        }

        private bool IsInGroup()
        {
            return IsUsingGroups() && m_GroupDepth > 0;
        }

        private void SortingLayerCheck(GameObject go)
        {
            // The game object may have custom properties that change how we sort
            CustomProperty property;
            if (go.TryGetCustomPropertySafe(StringConstants.Unity_SortingLayer, out property) || go.TryGetCustomPropertySafe(StringConstants.Unity_SortingLayerName, out property))
            {
                // Reset order on a new sorting layer
                var name = property.GetValueAsString();
                if (!string.Equals(name, m_CurrentSortLayerName, StringComparison.OrdinalIgnoreCase))
                {
                    m_CurrentSortLayerName = name;
                    m_CurrentSortOrder = 0;
                }
            }

            // The game object may have a custom property to hard-code the current sort order
            if (go.TryGetCustomPropertySafe(StringConstants.Unity_SortingOrder, out property))
            {
                m_CurrentSortOrder = property.GetValueAsInt();
            }
        }
    }
}
