using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class ObjectTemplate : ScriptableObject
    {
        public string m_ObjectXml;
        public SuperTile m_Tile;
        public List<CustomProperty> m_CustomProperties;
    }
}
