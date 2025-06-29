using SuperTiled2Unity.Ase.Editor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    internal sealed class TilesetAssetResolverAseprite : TilesetAssetResolver
    {
        public TilesetAssetResolverAseprite(string sourceAssetPath, TiledAssetImporter tiledAssetImporter, SuperTileset superTileset)
            : base(sourceAssetPath, tiledAssetImporter, superTileset)
        {
        }

        private class FrameTexture
        {
            public Texture2D Texture { get; set; }
            public int DurationMs { get; set; }
        }

        private AseFileVisitor m_AseFileVisitor;
        private readonly List<FrameTexture> m_FrameTextures = new List<FrameTexture>();

        public override bool AddSpritesAndTile(int tileId, int srcx, int srcy, int tileWidth, int tileHeight)
        {
            // We only add one tile but many sprites
            SuperTile tileToAdd = null;
            var fps = ST2USettings.instance.m_AnimationFramerate;
            var animationBuilder = new AnimationBuilder(fps);

            // Add sprites for each frame
            for (int i = 0; i < m_FrameTextures.Count; i++)
            {
                int x = srcx;
                int y = srcy;

                // In Tiled, texture origin is the top-left. However, in Unity the origin is bottom-left.
                y = (ExpectedHeight - y) - tileHeight;

                var frameTexture = m_FrameTextures[i];

                var sourceTexture = frameTexture.Texture;

                var assetName = Path.GetFileNameWithoutExtension(TiledAssetImporter.assetPath);
                var spriteName = $"{assetName}.Sprite.{tileId}.f{i}";

                var rect = new Rect(x, y, tileWidth, tileHeight);

                // Create and add the sprite that the tile is based off of
                var spriteToAdd = Sprite.Create(sourceTexture, rect, Vector2.zero, SuperTileset.m_PixelsPerUnit);
                spriteToAdd.name = spriteName;

                string spriteUniqueId = $"{spriteName}.{InternalId}";
                TiledAssetImporter.SuperImportContext.AddObjectToAsset(spriteUniqueId, spriteToAdd);

                // Keep track of the frame time on this sprite
                animationBuilder.AddFrames(spriteToAdd, frameTexture.DurationMs / 1000.0f);

                // Create and add the tile (only for the first sprite)
                if (i == 0)
                {
                    var tileName = $"{assetName}.Tile.{tileId}";
                    tileToAdd = SuperTile.CreateSuperTile();
                    tileToAdd.m_TileId = tileId;
                    tileToAdd.name = tileName;
                    tileToAdd.m_Sprite = spriteToAdd;
                    tileToAdd.m_Width = rect.width;
                    tileToAdd.m_Height = rect.height;
                    tileToAdd.m_TileOffsetX = SuperTileset.m_TileOffset.x;
                    tileToAdd.m_TileOffsetY = SuperTileset.m_TileOffset.y;
                    tileToAdd.m_ObjectAlignment = SuperTileset.m_ObjectAlignment;
                    tileToAdd.m_TileRenderSize = SuperTileset.m_TileRenderSize;
                    tileToAdd.m_FillMode = SuperTileset.m_FillMode;

                    SuperTileset.m_Tiles.Add(tileToAdd);

                    string tileUniqueId = $"{tileName}.{InternalId}";
                    TiledAssetImporter.SuperImportContext.AddObjectToAsset(tileUniqueId, tileToAdd);
                }
            }

            // Only animate if we have more than one frame
            if (tileToAdd != null && m_FrameTextures.Count > 1)
            {
                tileToAdd.m_AnimationSprites = animationBuilder.Sprites.ToArray();
            }

            return true;
        }

        protected override void OnPrepare()
        {
            var assetName = Path.GetFileNameWithoutExtension(SourceAssetPath);
            using (var reader = new AseReader(SourceAssetPath))
            {
                var aseFile = new AseFile(reader);

                m_AseFileVisitor = new AseFileVisitor();
                aseFile.VisitContents(m_AseFileVisitor);

                m_FrameTextures.Clear();
                m_FrameTextures.AddRange(m_AseFileVisitor.FrameCanvases.Select(f => new FrameTexture { Texture = f.Canvas.ToTexture2D(), DurationMs = f.DurationMs }));

                for (int i = 0; i < m_FrameTextures.Count; i++)
                {
                    m_FrameTextures[i].Texture.name = $"{assetName}.AseTexture.f{i}";
                    TiledAssetImporter.SuperImportContext.AddObjectToAsset(m_FrameTextures[i].Texture.name, m_FrameTextures[i].Texture);
                }
            }
        }

        protected override void OnDispose()
        {
            m_AseFileVisitor?.Dispose();
        }
    }
}
