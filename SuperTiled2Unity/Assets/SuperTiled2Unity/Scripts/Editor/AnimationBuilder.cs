using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2UnityEditor
{
    public class AnimationBuilder
    {
        private float m_Fps;
        private float m_Remainder;
        private List<Sprite> m_Sprites = new List<Sprite>();

        public AnimationBuilder(float fps)
        {
            m_Fps = fps;
        }

        public void AddFrames(Sprite sprite, float duration)
        {
            // fixit - this needs to be tested
            Assert.IsFalse(duration <= 0);

            // We may have time left over from the last time we added frames
            // This is time it will take to get to the next frame given our FPS

            if (m_Remainder >= duration)
            {
                // We will not be adding any frames but pay off duration
                m_Remainder -= duration;
                return;
            }
            else
            {
                // We will be adding some frames once we pay off debt from duration
                duration -= m_Remainder;

                float fNumFrames = duration * m_Fps;
                int iNumFrames = Mathf.CeilToInt(fNumFrames);

                m_Sprites.AddRange(Enumerable.Repeat(sprite, iNumFrames));
                m_Remainder = (iNumFrames - fNumFrames) / m_Fps; // fixit - is this right?
            }
        }
    }
}
