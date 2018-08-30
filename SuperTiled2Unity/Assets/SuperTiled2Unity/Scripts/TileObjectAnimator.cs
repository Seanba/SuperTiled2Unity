using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity
{
    public class TileObjectAnimator : MonoBehaviour
    {
        public float m_AnimationFramerate;
        public Sprite[] m_AnimationSprites;

        private float m_Timer;
        private int m_AnimationIndex;

        private void Update()
        {
            m_Timer += Time.deltaTime;
            float frameTime = 1.0f / m_AnimationFramerate;

            if (m_Timer > frameTime)
            {
                m_AnimationIndex++;
                if (m_AnimationIndex >= m_AnimationSprites.Length)
                {
                    m_AnimationIndex = 0;
                }

                var renderer = GetComponent<SpriteRenderer>();
                renderer.sprite = m_AnimationSprites[m_AnimationIndex];

                m_Timer = frameTime - m_Timer;
            }
        }
    }
}
