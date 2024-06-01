using System;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    partial class TmxAssetImporter
    {
        private SuperLayer ProcessImageLayer(GameObject goParent, XElement xLayer)
        {
            Assert.IsNotNull(xLayer);
            Assert.IsNotNull(goParent);

            // Create the game object that contains the layer and add it to the grid parent
            var layerComponent = goParent.AddSuperLayerGameObject<SuperImageLayer>(new SuperImageLayerLoader(xLayer, this), SuperImportContext);
            var goLayer = layerComponent.gameObject;

            AddSuperCustomProperties(goLayer, xLayer.Element("properties"));

            var xImage = xLayer.Element("image");
            if (xImage != null)
            {
                var source = xImage.GetAttributeAs<string>("source");
                layerComponent.m_ImageFilename = source;

                var tex2d = RequestDependencyAssetAtPath<Texture2D>(source);
                if (tex2d != null)
                {
                    // Create a sprite for the image
                    try
                    {
                        var sprite = Sprite.Create(tex2d, new Rect(0, 0, tex2d.width, tex2d.height), new Vector2(0, 1.0f), PixelsPerUnit);
                        SuperImportContext.AddObjectToAsset("_sprite", sprite);

                        var renderer = goLayer.AddComponent<SpriteRenderer>();
                        renderer.sprite = sprite;
                        renderer.color = layerComponent.CalculateColor();
                        AssignMaterial(renderer, layerComponent.m_TiledName);
                        AssignSpriteSorting(renderer);
                    }
                    catch (Exception e)
                    {
                        ReportGenericError($"Error creating sprite '{source}' for image layer '{layerComponent.m_TiledName}'\n{e.Message}");
                    }
                }
            }

            return layerComponent;
        }
    }
}
