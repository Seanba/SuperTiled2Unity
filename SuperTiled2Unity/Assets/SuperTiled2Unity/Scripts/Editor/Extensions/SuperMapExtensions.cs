using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class SuperMapExtensions
    {
        public static void UpdateProperties(this SuperMap superMap, SuperImportContext importContext)
        {
            var cellSize = superMap.CalculateCellSize();

            // The cell size has to take pixels per unit into account for x and y
            // z is always 1 so we can use Transparency Sort Axis
            float x = importContext.MakeScalar(cellSize.x);
            float y = importContext.MakeScalar(cellSize.y);
            float z = 1.0f;
            superMap.CellSize = new Vector3(x, y, z);
        }

        public static Vector2 GetTileAnchor(this SuperMap superMap)
        {
            if (superMap.m_Orientation == MapOrientation.Isometric)
            {
                return new Vector2(-0.5f, -0.5f);
            }

            return Vector2.zero;
        }

        public static Vector2 MapCoordinatesToPositionPPU(this SuperMap superMap, int cx, int cy)
        {
            float w = superMap.CellSize.x;
            float h = superMap.CellSize.y;

            if (superMap.m_Orientation == MapOrientation.Isometric)
            {
                var x = (cx - cy) * w * 0.5f;
                var y = (cx + cy) * h * 0.5f;
                return new Vector2(x, -y);
            }
            else
            {
                return new Vector2(cx * w, -cy * h);
            }
        }
    }
}
