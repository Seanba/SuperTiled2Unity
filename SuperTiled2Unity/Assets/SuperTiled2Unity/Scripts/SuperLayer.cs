using UnityEngine;

namespace SuperTiled2Unity
{
    public class SuperLayer : MonoBehaviour
    {
        [ReadOnly]
        public string m_TiledName;

        [ReadOnly]
        public float m_OffsetX;

        [ReadOnly]
        public float m_OffsetY;

        [ReadOnly]
        public float m_Opacity;

        [ReadOnly]
        public Color m_TintColor;

        [ReadOnly]
        public bool m_Visible;

        public Color CalculateColor()
        {
            Color color = Color.white;

            foreach (var layer in gameObject.GetComponentsInParent<SuperLayer>())
            {
                color *= layer.m_TintColor;
                color.a *= layer.m_Opacity;
            }

            return color;
        }

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
