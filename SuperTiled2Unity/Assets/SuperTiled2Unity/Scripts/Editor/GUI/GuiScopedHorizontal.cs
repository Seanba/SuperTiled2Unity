using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class GuiScopedHorizontal : IDisposable
    {
        public GuiScopedHorizontal()
        {
            GUILayout.BeginHorizontal();
        }

        public GuiScopedHorizontal(float space)
            : this()
        {
            GUILayout.Space(space);
        }

        public void Dispose()
        {
            GUILayout.EndHorizontal();
        }
    }
}
