using System;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class GuiScopedBackgroundColor : IDisposable
    {
        private Color m_DefaultColor;

        public GuiScopedBackgroundColor(Color color)
        {
            m_DefaultColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
        }

        public void Dispose()
        {
            GUI.backgroundColor = m_DefaultColor;
        }
    }
}
