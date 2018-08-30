using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class GuiScopedVertical : IDisposable
    {
        public GuiScopedVertical()
        {
            GUILayout.BeginVertical();
        }

        public GuiScopedVertical(float space)
            : this()
        {
            GUILayout.Space(space);
        }

        public void Dispose()
        {
            GUILayout.EndVertical();
        }
    }
}
