using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // Collection of icons to be used with SuperTiled2Unity assets
    public class SuperIcons : ScriptableSingleton<SuperIcons>
    {
        public Texture2D m_TmxIcon;
        public Texture2D m_TsxIcon;
        public Texture2D m_TxIcon;
        public Texture2D m_WorldIcon;
    }
}
