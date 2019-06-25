using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperTiled2Unity
{
    public enum FlipFlags
    {
        None = 0,

        Diagonal = 1,
        Vertical = 2,
        Horizontal = 4,
    }

    public static class FlipFlagsMask
    {
        public static bool FlippedHorizontally(FlipFlags flags)
        {
            return (flags & FlipFlags.Horizontal) != 0;
        }

        public static bool FlippedVertically(FlipFlags flags)
        {
            return (flags & FlipFlags.Vertical) != 0;
        }

        public static bool FlippedDiagonally(FlipFlags flags)
        {
            return (flags & FlipFlags.Diagonal) != 0;
        }
    }
}
