using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var layerComponent = goParent.AddSuperLayerGameObject<SuperImageLayer>(new SuperImageLayerLoader(xLayer), SuperImportContext);
            var goLayer = layerComponent.gameObject;

            AddSuperCustomProperties(goLayer, xLayer.Element("properties"));

            var xImage = xLayer.Element("image");
            if (xImage != null)
            {
                var source = xImage.GetAttributeAs<string>("source");
                layerComponent.m_ImageFilename = source;

                var tex2d = RequestAssetAtPath<Texture2D>(source);
                if (tex2d == null)
                {
                    // Texture was not found yet so report the error to the importer UI and bail
                    ReportError("Missing texture asset for image layer: {0}", source);
                }
                else
                {
                    // Create a sprite for the image
                    try
                    {
                        var sprite = Sprite.Create(tex2d, new Rect(0, 0, tex2d.width, tex2d.height), new Vector2(0, 1.0f), SuperImportContext.Settings.PixelsPerUnit);
                        SuperImportContext.AddObjectToAsset("_sprite", sprite);

                        var renderer = goLayer.AddComponent<SpriteRenderer>();
                        renderer.sprite = sprite;
                        renderer.color = new Color(1, 1, 1, layerComponent.CalculateOpacity());
                        AssignMaterial(renderer, layerComponent.m_TiledName);
                        AssignSpriteSorting(renderer);
                    }
                    catch (Exception e)
                    {
                        ReportError("Error creating sprite '{0}' for image layer '{1}'\n{2}", source, layerComponent.m_TiledName, e.Message);
                    }
                }
            }

            return layerComponent;
        }
    }
}
