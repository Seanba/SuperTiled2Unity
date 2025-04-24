using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace SuperTiled2Unity.Editor
{
    [ScriptedImporter(1, "st2U_atlas")]
    public class TilesetAtlasImporter : ScriptedImporter
    {
        public SpriteAtlas m_SpriteAtlas;
        public List<SuperAssetTileset> m_Tilesets = new List<SuperAssetTileset>();

        [MenuItem("Assets/Create/Super Tiled2Unity/Tileset Atlas")]
        private static void CreateMaterialFile()
        {
            ProjectWindowUtil.CreateAssetWithContent("TTileAtlas_new.st2u_atlas", "# Uses Super Tiled2Unity scripted importer for placing tileset sprites in a sprite atlas");
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var icon = SuperIcons.instance.m_TilesetAtlasIcon;
            var tilesetSprites = ScriptableObject.CreateInstance<TilesetSprites>();
            ctx.AddObjectToAsset("_tileset_sprites", tilesetSprites, icon);
            ctx.SetMainObject(tilesetSprites);

            // If there is not sprite atlas then we're done
            if (m_SpriteAtlas == null)
            {
                return;
            }

            // Remove all the previous sprites from the atlas
            SpriteAtlasExtensions.Remove(m_SpriteAtlas, SpriteAtlasExtensions.GetPackables(m_SpriteAtlas));

            // Go through all the sprites in all our tilesets and add them to the sprite atlas
            foreach (var tileset in m_Tilesets)
            {
                var tilesetAssetPath = AssetDatabase.GetAssetPath(tileset);
                if (!string.IsNullOrEmpty(tilesetAssetPath))
                {
                    tilesetSprites.m_Sprites.AddRange(AssetDatabase.LoadAllAssetsAtPath(tilesetAssetPath).OfType<Sprite>());

                    // If the tileset changes then the atlas will be automatically updated
                    ctx.DependsOnArtifact(tilesetAssetPath);
                }
            }

            SpriteAtlasExtensions.Add(m_SpriteAtlas, tilesetSprites.m_Sprites.ToArray());
        }
    }
}
