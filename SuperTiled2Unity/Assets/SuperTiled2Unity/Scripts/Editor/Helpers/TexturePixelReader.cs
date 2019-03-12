using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // Utility class that helps us read pixels from a source texture, even if that source is not enabled for read/write
    // See this: https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
    public class TexturePixelReader : IDisposable
    {
        private Texture2D m_SourceCopy;

        public TexturePixelReader(Texture2D source)
        {
            // Create a tempoary texture that has readable texture data. We will copy from that texture to our target.
            RenderTexture tmp = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(source, tmp);

            // Keep track of active render texture and push our temp
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;

            // Copy the source texture into our copy
            m_SourceCopy = new Texture2D(source.width, source.height);
            m_SourceCopy.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            m_SourceCopy.Apply();

            // Pop our temporary
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);
        }

        public Color[] GetPixels(Rect rc)
        {
            return GetPixels((int)rc.x, (int)rc.y, (int)rc.width, (int)rc.height);
        }

        public Color[] GetPixels(int x, int y, int blockWidth, int blockHeight)
        {
            return m_SourceCopy.GetPixels(x, y, blockWidth, blockHeight);
        }

        public void Dispose()
        {
            UnityEngine.Object.DestroyImmediate(m_SourceCopy);
        }
    }
}
