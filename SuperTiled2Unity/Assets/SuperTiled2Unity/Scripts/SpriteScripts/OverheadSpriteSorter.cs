using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity
{
    // Helper class to sort a sprite in real-time as they traverse a map
    // (It just sets the sorting order on a character as they move around)
    public class OverheadSpriteSorter : MonoBehaviour
    {
        private SpriteRenderer m_SpriteRenderer;

        private void Awake()
        {
            m_SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            Assert.IsNotNull(m_SpriteRenderer);
        }

        private void LateUpdate()
        {
            // Transform by pixels per unit and negate (sorting order increases as y goes down)
            float y = -transform.position.y;
            y *= m_SpriteRenderer.sprite.pixelsPerUnit;

            m_SpriteRenderer.sortingOrder = (int)y;
        }
    }
}
