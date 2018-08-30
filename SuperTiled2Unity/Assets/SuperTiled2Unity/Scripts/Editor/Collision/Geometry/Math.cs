using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity.Editor.Geometry
{
    public static class GeoMath
    {
        // Points are ordered CCW with B as the junction
        public static float Cross(Vector2 A, Vector2 B, Vector2 C)
        {
            Vector2 lhs = new Vector2(B.x - A.x, B.y - A.y);
            Vector2 rhs = new Vector2(C.x - B.x, C.y - B.y);
            return (lhs.x * rhs.y) - (lhs.y * rhs.x);
        }
    }
}
