using UnityEngine;

namespace SuperTiled2Unity
{
    public enum ObjectAlignment
    {
        Unspecified,
        TopLeft,
        Top,
        TopRight,
        Left,
        Center,
        Right,
        BottomLeft,
        Bottom,
        BottomRight,
    }

    public static class ObjectAlignmentToPivot
    {
        public static Vector3 ToVector3(float width, float height, float inversePPU, MapOrientation mapOrientation, ObjectAlignment objectAlignment)
        {
            width *= inversePPU;
            height *= inversePPU;

            float dx = 0;
            float dy = 0;

            if (objectAlignment == ObjectAlignment.Unspecified)
            {
                objectAlignment = mapOrientation == MapOrientation.Isometric ? ObjectAlignment.Bottom : ObjectAlignment.BottomLeft;
            }

            switch (objectAlignment)
            {
                case ObjectAlignment.BottomLeft:
                    dx = 0;
                    dy = 0;
                    break;

                case ObjectAlignment.Top:
                    dx = -width * 0.5f;
                    dy = -height;
                    break;

                case ObjectAlignment.TopRight:
                    dx = -width;
                    dy = -height;
                    break;

                case ObjectAlignment.TopLeft:
                    dx = 0;
                    dy = -height;
                    break;

                case ObjectAlignment.Left:
                    dx = 0;
                    dy = -height * 0.5f;
                    break;

                case ObjectAlignment.Center:
                    dx = -width * 0.5f;
                    dy = -height * 0.5f;
                    break;

                case ObjectAlignment.Right:
                    dx = -width;
                    dy = -height * 0.5f;
                    break;

                case ObjectAlignment.Bottom:
                    dx = -width * 0.5f;
                    dy = 0;
                    break;

                case ObjectAlignment.BottomRight:
                    dx = -width;
                    dy = 0;
                    break;
            }

            return new Vector3(dx, dy, 0);
        }
    }
}
