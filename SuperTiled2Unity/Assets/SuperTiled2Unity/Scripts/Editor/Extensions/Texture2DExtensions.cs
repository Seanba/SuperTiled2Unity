using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class Texture2DExtensions
    {
        public static void BlitRectFrom(this Texture2D texture, int dx, int dy, Texture2D sourceTexture, Rect sourceRect)
        {
            var format = sourceTexture.format;
            if (format == TextureFormat.ARGB32 || format == TextureFormat.BGRA32 || format == TextureFormat.RGBA32)
            {
                // Order of magnitude faster
                Graphics.CopyTexture(sourceTexture, 0, 0, (int)sourceRect.x, (int)sourceRect.y, (int)sourceRect.width, (int)sourceRect.height, texture, 0, 0, dx, dy);
            }
            else
            {
                // So, why do we create a temporary render texture? Because the source texture may not be enabled for reading.
                // See this: https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-

                // Create a tempoary texture that has readable texture data. We will copy from that texture to our target.
                RenderTexture tmp = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                Graphics.Blit(sourceTexture, tmp);

                // Keep track of active render texture and push our temp
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = tmp;

                // Copy the source texture into our copy
                Texture2D sourceTextureCopy = new Texture2D(sourceTexture.width, sourceTexture.height);
                sourceTextureCopy.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                sourceTextureCopy.Apply();

                // Pop our temporary
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tmp);

                // Do the actual Blit (from a copy of our source)
                int sx = (int)sourceRect.x;
                int sy = (int)sourceRect.y;
                int sw = (int)sourceRect.width;
                int sh = (int)sourceRect.height;

                var sourcePixels = sourceTextureCopy.GetPixels(sx, sy, sw, sh);
                texture.SetPixels(dx, dy, sw, sh, sourcePixels);

                // Destroy our source copy
                Texture2D.DestroyImmediate(sourceTextureCopy);
            }
        }

        public static void BlitRectFrom(this Texture2D texture, float dx, float dy, Texture2D sourceTexture, Rect sourceRect)
        {
            texture.BlitRectFrom((int)dx, (int)dy, sourceTexture, sourceRect);
        }

        public static void CopyOwnPixels(this Texture2D texture, int dx, int dy, Rect sourceRect)
        {
            // Take for granted that our own texture is read/write enabled
            int sx = (int)sourceRect.x;
            int sy = (int)sourceRect.y;
            int sw = (int)sourceRect.width;
            int sh = (int)sourceRect.height;

            var pixels = texture.GetPixels(sx, sy, sw, sh);
            //pixels = pixels.Select(p => NamedColors.Purple).ToArray();

            texture.SetPixels(dx, dy, sw, sh, pixels);
        }

        public static void CopyOwnPixels(this Texture2D texture, float dx, float dy, Rect sourceRect)
        {
            texture.CopyOwnPixels((int)dx, (int)dy, sourceRect);
        }
    }
}
