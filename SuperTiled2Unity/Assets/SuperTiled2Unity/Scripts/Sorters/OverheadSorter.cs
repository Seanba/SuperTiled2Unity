using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity
{
    public class OverheadSorter : MonoBehaviour
    {
        private SpriteRenderer m_Renderer;

        private void Awake()
        {
            m_Renderer = GetComponentInChildren<SpriteRenderer>();
        }

        protected void SortOnYPosition()
        {
            m_Renderer.sortingOrder = -(int)(m_Renderer.sprite.pixelsPerUnit * transform.position.y);
        }
    }
}
