using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class Clamper
    {
        public static float ClampPixelsPerUnit(float value)
        {
            return Mathf.Clamp(value, 0.01f, 2048.0f);
        }

        public static int ClampEdgesPerEllipse(int value)
        {
            return Mathf.Clamp(value, 6, 256);
        }

        public static int ClampAnimationFramerate(int value)
        {
            return Mathf.Clamp(value, 1, 125);
        }
    }
}
