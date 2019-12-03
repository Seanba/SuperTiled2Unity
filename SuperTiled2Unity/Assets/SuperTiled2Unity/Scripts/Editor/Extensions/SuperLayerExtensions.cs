using UnityEngine;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity.Editor
{
    public static class SuperLayerExtensions
    {
        public static void SetWorldPosition(this SuperLayer layer, SuperMap map, SuperImportContext context)
        {
            // Accumlate positions up the tree
            Vector3 position_w = new Vector3();
            foreach (var parent in layer.gameObject.GetComponentsInParent<SuperLayer>())
            {
                position_w += (Vector3)context.MakePoint(parent.m_OffsetX, parent.m_OffsetY);
                position_w.z += parent.gameObject.GetSuperPropertyValueFloat(StringConstants.Unity_ZPosition, 0);
            }

            // Add an additional offset if our tileset is present. These coordinates have already been transformed.
            if (layer is SuperTileLayer || layer.GetComponent<Tilemap>() != null || layer.GetComponent<SuperTilesAsObjectsTilemap>() != null)
            {
                position_w += context.TilemapOffset;
            }

            layer.transform.position = position_w;

            // This sucks but we have to correct for isometric orientation for image layers
            if (layer is SuperImageLayer && map.m_Orientation == MapOrientation.Isometric)
            {
                float dx = context.MakeScalar(map.m_Height * map.m_TileHeight);
                layer.transform.Translate(-dx, 0, 0);
            }
        }
    }
}
