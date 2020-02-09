using UnityEngine;

namespace SuperTiled2Unity
{
    public class SuperObject : MonoBehaviour
    {
        [ReadOnly]
        public int m_Id;

        [ReadOnly]
        public string m_TiledName;

        [ReadOnly]
        public string m_Type;

        [ReadOnly]
        public float m_X;

        [ReadOnly]
        public float m_Y;

        [ReadOnly]
        public float m_Width;

        [ReadOnly]
        public float m_Height;

        [ReadOnly]
        public float m_Rotation;

        [ReadOnly]
        public uint m_TileId;

        [ReadOnly]
        public SuperTile m_SuperTile;

        [ReadOnly]
        public bool m_Visible;

        [ReadOnly]
        public string m_Template;

        public float CalculateOpacity()
        {
            float opacity = 1.0f;

            foreach (var layer in gameObject.GetComponentsInParent<SuperLayer>())
            {
                opacity *= layer.m_Opacity;
            }

            return opacity;
        }
    }
}
