using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class AsepriteAssetPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessAsset()
        {
            if (assetImporter is AsepriteImporter aseImporter)
            {
                aseImporter.OnPostAsepriteImport += OnPostAsepriteImport;
            }
        }

        private static void OnPostAsepriteImport(AsepriteImporter.ImportEventArgs args)
        {
            args.importer.OnPostAsepriteImport -= OnPostAsepriteImport;

            var objects = new List<UnityEngine.Object>();
            args.context.GetObjects(objects);

            if (!objects.OfType<Sprite>().Any())
            {
                // We must have at least one sprite in the Aseprite asset
                // But we may not have one depending on our import options (i.e. Sprite Sheet Import Mode)
                // In this case we make a sprite out of the texture atlas within the asset
                // This will result in tiles that do not animate but so be it
                var texture2d = objects.OfType<Texture2D>().FirstOrDefault();
                if (texture2d != null)
                {
                    float x = args.importer.mosaicPadding;
                    float y = args.importer.mosaicPadding;
                    float w = args.importer.canvasSize.x;
                    float h = args.importer.canvasSize.y;
                    var rect = new Rect(x, y, w, h);
                    var sprite = Sprite.Create(texture2d, rect, Vector2.zero);
                    sprite.name = "st2u.ImportedSpriteSheet";
                    args.context.AddObjectToAsset("_st2u-imported-sprite-sheet", sprite);
                }
            }
        }
    }
}
