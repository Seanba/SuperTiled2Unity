using System;
using Unity.Collections;
using UnityEngine;

namespace SuperTiled2Unity.Ase.Editor
{
    // Drawing board for aseprite pixels, blend operations, etc.
    internal class AseCanvas : IDisposable
    {
        public int Width { get; }
        public int Height { get; }
        public NativeArray<Color32> Pixels { get; }

        public AseCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new NativeArray<Color32>(Width * Height, Allocator.Persistent);
        }

        public Texture2D ToTexture2D()
        {
            // Create the texture from raw cavnas pixels
            var texture2d = new Texture2D(Width, Height, TextureFormat.ARGB32, false);
            texture2d.wrapMode = TextureWrapMode.Clamp;
            texture2d.filterMode = FilterMode.Point;
            texture2d.alphaIsTransparency = true;
            
            texture2d.SetPixels32(Pixels.ToArray(), 0);
            texture2d.Apply();

            // The Aseprite cooridnate system is upside-down from the perspective of Unity
            // So we do a Graphics.Blit to make it right side up
            var renderTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGB32, 0);
            renderTexture.wrapMode = TextureWrapMode.Clamp;
            renderTexture.filterMode = FilterMode.Point;
            RenderTexture oldRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            {
                Graphics.Blit(texture2d, renderTexture, new Vector2(1, -1), new Vector2(0, 1));
                texture2d.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                texture2d.Apply();
            }
            RenderTexture.active = oldRenderTexture;

            return texture2d;
        }

        public void Dispose()
        {
            Pixels.Dispose();
        }
    }
}
