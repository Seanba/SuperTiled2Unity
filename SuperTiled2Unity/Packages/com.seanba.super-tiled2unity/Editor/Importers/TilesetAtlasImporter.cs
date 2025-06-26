using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

// fixit - Rules
// [2020.3-2021.3] Can only support sprite atlas v1 (anything before 2021.3)
// Other versions: EditorSettings.spritePackerMode must match v1 or v2

namespace SuperTiled2Unity.Editor
{
    [ScriptedImporter(1, "st2U_atlas")]
    public class TilesetAtlasImporter : ScriptedImporter
    {
        public SpriteAtlas m_SpriteAtlas;
        public List<SuperTileset> m_SuperTiled2UnityTilesets = new List<SuperTileset>();

        [MenuItem("Assets/Create/Super Tiled2Unity/Tileset Atlas")]
        private static void CreateMaterialFile()
        {
            ProjectWindowUtil.CreateAssetWithContent("TTileAtlas_new.st2u_atlas", "# Uses Super Tiled2Unity scripted importer for placing tileset sprites in a sprite atlas");
        }

        public string GetSpriteAtlasAssetPath()
        {
            return AssetDatabase.GetAssetPath(m_SpriteAtlas);
        }

        public bool IsUnityVersionIncompatible()
        {
#if !UNITY_2021_3_OR_NEWER
            return IsUsingSpriteAtlasV2();
#else
            return false;
#endif
        }

        public bool IsEditorVersionIncompatibleV1() // fixit - support this
        {
            if (EditorSettings.spritePackerMode == SpritePackerMode.SpriteAtlasV2 && !IsUsingSpriteAtlasV2())
            {
                return true;
            }

            return false;
        }

        public bool IsUsingSpriteAtlasV2()
        {
            var assetPath = AssetDatabase.GetAssetPath(m_SpriteAtlas);
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            return assetPath.EndsWith(".spriteatlasv2", StringComparison.OrdinalIgnoreCase);
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var icon = SuperIcons.instance.m_TilesetAtlasIcon;
            var tilesetSprites = ScriptableObject.CreateInstance<TilesetSprites>();
            ctx.AddObjectToAsset("_tileset_sprites", tilesetSprites, icon);
            ctx.SetMainObject(tilesetSprites);

            // If there is not a sprite atlas then we're done
            if (m_SpriteAtlas == null)
            {
                return;
            }

            if (IsUnityVersionIncompatible())
            {
                Debug.LogError("You Unity version does not support Tileset Atlases and Sprite Packer V2. Need Unity 2021.3 or later.");
                return;
            }


            // Go through all the sprites in all our tilesets and add them to the sprite atlas
            foreach (var tileset in m_SuperTiled2UnityTilesets)
            {
                var tilesetAssetPath = AssetDatabase.GetAssetPath(tileset);
                if (!string.IsNullOrEmpty(tilesetAssetPath))
                {
                    tilesetSprites.m_Sprites.AddRange(AssetDatabase.LoadAllAssetsAtPath(tilesetAssetPath).OfType<Sprite>());

                    // If the tileset changes then the atlas will be automatically updated
                    ctx.DependsOnArtifact(tilesetAssetPath);
                }
            }

            AssignSpritesToAtlas(tilesetSprites.m_Sprites);
        }

        private void AssignSpritesToAtlas(IEnumerable<Sprite> sprites)
        {
            //EditorSettings.spritePackerMode == SpritePackerMode.SpriteAtlasV2;

            // fixit - you cannot downgrade to version 1 of the sprite atlas, ffs
            var spriteAtlasAssetPath = AssetDatabase.GetAssetPath(m_SpriteAtlas);
            var oldPackables = m_SpriteAtlas.GetPackables();

#if UNITY_2021_3_OR_NEWER
            var spriteAtlasAsset = SpriteAtlasAsset.Load(spriteAtlasAssetPath);
            if (spriteAtlasAsset != null)
            {
                // fixit This works for V2 in 2021.3.11 (test for minimal version)
                spriteAtlasAsset.Remove(oldPackables);
                spriteAtlasAsset.Add(sprites.ToArray());
                SpriteAtlasAsset.Save(spriteAtlasAsset, spriteAtlasAssetPath);
            }
            else
#endif
            {
                // Version 1
                SpriteAtlasExtensions.Remove(m_SpriteAtlas, oldPackables);
                SpriteAtlasExtensions.Add(m_SpriteAtlas, sprites.ToArray());
            }
        }
    }
}
