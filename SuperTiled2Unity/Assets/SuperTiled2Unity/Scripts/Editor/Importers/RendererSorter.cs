﻿using System;
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

        public void EnterObjectLayer(SuperObjectLayer layer)
        {
            SortingLayerCheck(layer.gameObject);
        }

        public string AssignSort(Renderer renderer)
        {
            var go = renderer.gameObject;

            SortingLayerCheck(go);
            renderer.sortingLayerName = m_CurrentSortLayerName;
            renderer.sortingOrder = m_CurrentSortOrder++; // fixit - but not if in a group

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