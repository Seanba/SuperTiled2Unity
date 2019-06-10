using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class SuperMapExtensions
    {
        public static Vector2 CellPositionToLocalPosition(this SuperMap superMap, int cx, int cy)
        {
            var grid = superMap.GetComponentInChildren<Grid>();
            return grid.CellToLocal(new Vector3Int(cx, cy, 0));
        }
    }
}
