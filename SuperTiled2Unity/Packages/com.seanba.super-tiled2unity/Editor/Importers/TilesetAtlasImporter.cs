using System;
using System.Collections.Generic;
using System.IO;
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
        public List<SuperTileset> m_SuperTiled2UnityTilesets = new List<SuperTileset>();

        [MenuItem("Assets/Create/Super Tiled2Unity/Tileset Atlas")]
        private static void CreateMaterialFile()
        {
            ProjectWindowUtil.CreateAssetWithContent("TTileAtlas_new.st2u_atlas", "# Uses Super Tiled2Unity scripted importer for placing tileset sprites in a sprite atlas");
        }

        // Unity Sprite Atlas programming is tricky because of V1 and V2 and the feeling that neither is well supported by Unity APIs
        public bool SpriteAtlasWillFail(out string reason)
        {
            string atlasAssetPath = AssetDatabase.GetAssetPath(m_SpriteAtlas);
            string atlasAssetFileName = Path.GetFileName(atlasAssetPath);
            string atlasAssetFileNameWithoutExtension = Path.GetFileNameWithoutExtension(atlasAssetFileName);
            bool isV2 = atlasAssetPath.EndsWith(".spriteatlasv2", StringComparison.OrdinalIgnoreCase);
            bool isEditorV2 = EditorSettings.spritePackerMode == SpritePackerMode.SpriteAtlasV2;

            if (isV2 && !isEditorV2)
            {
                reason = $"'{atlasAssetFileNameWithoutExtension}' uses Sprite Atlas V2 but the project settings use Sprite Atlas V1.\nSee: Project Settings / Editor / Sprite Packer";

#if !UNITY_2021_3_OR_NEWER
                reason += "\n\nNote you need at least Unity 2021.3 in order to use Tileset Atlas with Sprite Atlas V2.";
#endif
                return true;
            }

            if (isEditorV2 && !isV2)
            {
                reason = $"'{atlasAssetFileNameWithoutExtension}' uses Sprite Atlas V1 but the project settings use Sprite Atlas V2.\nSee: Project Settings / Editor / Sprite Packer";
                return true;
            }

#if !UNITY_2021_3_OR_NEWER
            if (isV2)
            {
                reason = $"'{atlasAssetFileNameWithoutExtension}' uses Sprite Atlas V2.\nSprite Atlas V2 is not supported with TilesetAtlas and this version of Unity.\nUpgrade to Unity 2021.3 or use Sprite Atlas V1.\nSee: Project Settings / Editor / Sprite Packer";
                return true;
            }
#endif

            reason = string.Empty;
            return false;
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

            if (SpriteAtlasWillFail(out string reason))
            {
                // We can't assign sprites to the sprite atlas
                Debug.LogError(reason);
                return;
            }

            AssignSpritesToAtlas(tilesetSprites.m_Sprites);
        }

        private void AssignSpritesToAtlas(IEnumerable<Sprite> sprites)
        {
            var spriteAtlasAssetPath = AssetDatabase.GetAssetPath(m_SpriteAtlas);
            var oldPackables = m_SpriteAtlas.GetPackables();

#if UNITY_2021_3_OR_NEWER
            var spriteAtlasAsset = SpriteAtlasAsset.Load(spriteAtlasAssetPath);
            if (spriteAtlasAsset != null)
            {
                // Version 2
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
