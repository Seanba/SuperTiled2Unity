using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace SuperTiled2Unity.Editor
{
    public class GuiScopedIndent : IDisposable
    {
        public GuiScopedIndent()
        {
            EditorGUI.indentLevel++;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel--;
        }
    }
}
