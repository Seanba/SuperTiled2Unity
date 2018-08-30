using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class RectUtil
    {
        public static Rect LeftEdge(Rect rc)
        {
            return new Rect(rc.x, rc.y, 1, rc.height);
        }

        public static Rect RightEdge(Rect rc)
        {
            return new Rect(rc.xMax - 1, rc.y, 1, rc.height);
        }

        public static Rect TopEdge(Rect rc)
        {
            return new Rect(rc.x, rc.yMax - 1, rc.width, 1);
        }

        public static Rect BottomEdge(Rect rc)
        {
            return new Rect(rc.x, rc.y, rc.width, 1);
        }

    }
}
