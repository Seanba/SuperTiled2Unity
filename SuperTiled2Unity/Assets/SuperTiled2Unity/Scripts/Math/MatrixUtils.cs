using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity
{
    public static class MatrixUtils
    {
        public static Matrix4x4 Rotate2d(float m00, float m01, float m10, float m11)
        {
            var mat = Matrix4x4.identity;
            mat.m00 = m00;
            mat.m01 = m01;
            mat.m10 = m10;
            mat.m11 = m11;

            return mat;
        }
    }
}
