using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity.Editor
{
    // Heper API for sorting Tilemap and Sprite renderers
    public class RendererSorting
    {
        private const string DefaultSortingLayerName = "Default";

        private TmxAssetImporter m_TmxAssetImporter;
        private Stack<int> m_GroupingOrder;

        public RendererSorting(TmxAssetImporter tmxImporter)
        {
            m_TmxAssetImporter = tmxImporter;

            // Default starting group
            m_GroupingOrder = new Stack<int>();
            m_GroupingOrder.Push(0);
        }

        public  void PushGrouping()
        {
            m_GroupingOrder.Push(0);
        }

        public void PopGrouping()
        {
            m_GroupingOrder.Pop();
        }

        public void SortNewTilemapRenderer(TilemapRenderer renderer)
        {
            var go = renderer.gameObject;

            renderer.sortingLayerName = GetSortingLayerName(go);
            renderer.sortingOrder = m_GroupingOrder.Peek();

            // The next Tilemap goes up in sort order
            IncrementGroupOrder();
        }

        private void IncrementGroupOrder()
        {
            int order = m_GroupingOrder.Pop();
            m_GroupingOrder.Push(order + 1);
        }

        private string GetSortingLayerName(GameObject go)
        {
            // Does this game object have a custom property for the sorting layer name?
            CustomProperty property;

            if (go.TryGetCustomPropertySafe("unity:SortingLayer", out property) ||
                go.TryGetCustomPropertySafe("unity:sortingLayerName", out property))
            {
                return property.GetValueAsString();
            }

            return DefaultSortingLayerName;
        }
    }
}
