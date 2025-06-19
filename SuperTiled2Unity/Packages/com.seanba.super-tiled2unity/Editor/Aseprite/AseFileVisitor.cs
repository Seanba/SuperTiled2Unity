using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace SuperTiled2Unity.Ase.Editor
{
    internal class AseFileVisitor : IAseVisitor, IDisposable
    {
        public class FrameCanvas
        {
            public AseCanvas Canvas { get; }
            public int DurationMs { get; }

            public FrameCanvas(AseCanvas canvas, int durationMs)
            {
                Canvas = canvas;
                DurationMs = durationMs;
            }
        }

        public int CanvasWidth => m_AseFile.Header.Width;
        public int CanvasHeight => m_AseFile.Header.Height;
        public ColorDepth ColorDepth => m_AseFile.Header.ColorDepth;
        public int TransparentIndex => m_AseFile.Header.TransparentIndex;

        public Stack<FrameCanvas> FrameCanvases { get; } = new Stack<FrameCanvas>();

        private AseFile m_AseFile;
        private readonly List<AseLayerChunk> m_LayerChunks = new List<AseLayerChunk>();
        private readonly List<AseTilesetChunk> m_TilesetChunks = new List<AseTilesetChunk>();
        private readonly AseGraphics.GetPixelArgs m_GetPixelArgs = new AseGraphics.GetPixelArgs();

        public void Dispose()
        {
            foreach (var frame in FrameCanvases)
            {
                frame.Canvas.Dispose();
            }
        }

        public void BeginFileVisit(AseFile file)
        {
            m_AseFile = file;
            m_GetPixelArgs.ColorDepth = ColorDepth;
        }

        public void EndFileVisit(AseFile file)
        {
            m_AseFile = null;
        }

        public void BeginFrameVisit(AseFrame frame)
        {
            // Create a new blank canvas to be written to for the frame
            var canvas = new AseCanvas(CanvasWidth, CanvasHeight);
            FrameCanvases.Push(new FrameCanvas(canvas, frame.FrameDurationMs));
        }

        public void EndFrameVisit(AseFrame frame)
        {
            // Any cleanup needed?
        }

        public void VisitLayerChunk(AseLayerChunk layer)
        {
            m_LayerChunks.Add(layer);
        }

        public void VisitCelChunk(AseCelChunk cel)
        {
            var layer = m_LayerChunks[cel.LayerIndex];
            if (!layer.IsVisible)
            {
                // Ignore cels from invisible layers
                return;
            }

            if (cel.LinkedCel != null)
            {
                cel = cel.LinkedCel;
            }

            if (cel.CelType == CelType.CompressedImage)
            {
                // Get the pixels from this cel and blend them into the canvas for this frame
                unsafe
                {
                    var canvas = FrameCanvases.Peek().Canvas;
                    var canvasPixels = (Color32*)canvas.Pixels.GetUnsafePtr();

                    m_GetPixelArgs.PixelBytes = cel.PixelBytes;
                    m_GetPixelArgs.Stride = cel.Width;

                    for (int x = 0; x < cel.Width; x++)
                    {
                        for (int y = 0; y < cel.Height; y++)
                        {
                            Color32 celPixel = AseGraphics.GetPixel(x, y, m_GetPixelArgs);
                            celPixel.a = AseGraphics.CalculateOpacity(celPixel.a, layer.Opacity, cel.Opacity);
                            if (celPixel.a > 0)
                            {
                                int cx = cel.PositionX + x;
                                int cy = cel.PositionY + y;
                                int index = cx + (cy * canvas.Width);

                                Color32 basePixel = canvasPixels[index];
                                Color32 blendedPixel = AseGraphics.BlendColors(layer.BlendMode, basePixel, celPixel);
                                canvasPixels[index] = blendedPixel;
                            }
                        }
                    }
                }
            }
            else if (cel.CelType == CelType.CompressedTilemap)
            {
                // Find layer that is a Tilemap type and has a matching Tileset Index
                var tileset = m_TilesetChunks.FirstOrDefault(ts => ts.TilesetId == layer.TilesetIndex);
                if (tileset != null)
                {
                    unsafe
                    {
                        var canvas = FrameCanvases.Peek().Canvas;
                        var canvasPixels = (Color32*)canvas.Pixels.GetUnsafePtr();

                        m_GetPixelArgs.PixelBytes = tileset.PixelBytes;
                        m_GetPixelArgs.Stride = tileset.TileWidth;

                        for (int t = 0; t < cel.TileData32.Length; t++)
                        {
                            // A tileId of zero means an empty tile
                            int tileId = (int)cel.TileData32[t];
                            if (tileId != 0)
                            {
                                int tile_i = t % cel.NumberOfTilesWide;
                                int tile_j = t / cel.NumberOfTilesWide;

                                // What are the start and end coordinates for the tile?
                                int txmin = 0;
                                int txmax = txmin + tileset.TileWidth;
                                int tymin = tileId * tileset.TileHeight;
                                int tymax = tymin + tileset.TileHeight;

                                // What are the start and end coordinates for the canvas we are copying tile pixels to?
                                int cxmin = cel.PositionX + (tile_i * tileset.TileWidth);
                                int cxmax = Math.Min(canvas.Width, cxmin + tileset.TileWidth);
                                int cymin = cel.PositionY + (tile_j * tileset.TileHeight);
                                int cymax = Math.Min(canvas.Height, cymin + tileset.TileHeight);

                                for (int tx = txmin, cx = cxmin; tx < txmax && cx < cxmax; tx++, cx++)
                                {
                                    for (int ty = tymin, cy = cymin; ty < tymax && cy < cymax; ty++, cy++)
                                    {
                                        Color32 tilePixel = AseGraphics.GetPixel(tx, ty, m_GetPixelArgs);
                                        tilePixel.a = AseGraphics.CalculateOpacity(tilePixel.a, layer.Opacity, cel.Opacity);
                                        if (tilePixel.a > 0)
                                        {
                                            int canvasPixelIndex = cx + (cy * canvas.Width);
                                            Color32 basePixel = canvasPixels[canvasPixelIndex];
                                            Color32 blendedPixel = AseGraphics.BlendColors(layer.BlendMode, basePixel, tilePixel);
                                            canvasPixels[canvasPixelIndex] = blendedPixel;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Cannot find tileset {layer.TilesetIndex} for layer {layer.Name}");
                }
            }
        }

        public void VisitDummyChunk(AseDummyChunk dummy)
        {
            // Ignore dummy chunks
        }

        public void VisitFrameTagsChunk(AseFrameTagsChunk frameTags)
        {
        }

        public void VisitOldPaletteChunk(AseOldPaletteChunk palette)
        {
            m_GetPixelArgs.Palette.Clear();
            m_GetPixelArgs.Palette.AddRange(palette.Colors.Select(c => new Color32(c.red, c.green, c.blue, 255)));
            m_GetPixelArgs.Palette[TransparentIndex] = Color.clear;
        }

        public void VisitPaletteChunk(AsePaletteChunk palette)
        {
            m_GetPixelArgs.Palette.Clear();
            m_GetPixelArgs.Palette.AddRange(palette.Entries.Select(e => new Color32(e.Red, e.Green, e.Blue, e.Alpha)));
            m_GetPixelArgs.Palette[TransparentIndex] = Color.clear;
        }

        public void VisitSliceChunk(AseSliceChunk slice)
        {
        }

        public void VisitTilesetChunk(AseTilesetChunk tileset)
        {
            m_TilesetChunks.Add(tileset);
        }

        public void VisitUserDataChunk(AseUserDataChunk userData)
        {
        }
    }
}
