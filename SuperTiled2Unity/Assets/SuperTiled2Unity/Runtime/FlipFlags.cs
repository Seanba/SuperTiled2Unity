namespace SuperTiled2Unity
{
    public enum FlipFlags
    {
        None = 0,

        Diagonal = 1,
        Vertical = 2,
        Horizontal = 4,
        Hexagonal120 = 8,
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

        public static bool RotatedDiagonally(FlipFlags flags)
        {
            return (flags & FlipFlags.Diagonal) != 0;
        }

        public static bool RotatedHexagonally120(FlipFlags flags)
        {
            return (flags & FlipFlags.Hexagonal120) != 0;
        }
    }
}
