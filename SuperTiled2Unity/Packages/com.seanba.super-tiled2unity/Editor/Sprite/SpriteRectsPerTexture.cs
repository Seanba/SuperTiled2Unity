using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity
{
    // How a texture 2d resource is to be cut up into sprites
    [Serializable]
    public class SpriteRectsPerTexture
    {
        [SerializeField]
        public Texture2D m_Texture2D; // fixit - don't think this works because it is nulled out while texture is (re)imported

        [SerializeField]
        public string m_TextureAssetPath;

        [SerializeField]
        public List<SpriteRect> m_SpriteRects = new List<SpriteRect>();
    }
}
