using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // Tilesets can be stand-alone TSX files (preferred) or embedded in Tiled Map Editor files (TMX)
    // This helper loader class gives us the flexibility we need to load tileset data
    public class TilesetLoader
    {
        private SuperTileset m_TilesetScript;
        private TiledAssetImporter m_Importer;
        private bool m_UseSpriteAtlas;
        private int m_AtlasWidth;
        private int m_AtlasHeight;

        public TilesetLoader(SuperTileset tileset, TiledAssetImporter importer, bool useAtlas, int atlasWidth, int atlasHeight)
        {
            m_TilesetScript = tileset;
            m_Importer = importer;
            m_UseSpriteAtlas = useAtlas;
            m_AtlasWidth = atlasWidth;
            m_AtlasHeight = atlasHeight;
        }

        public bool LoadFromXml(XElement xTileset)
        {
            // There are attributes we *must* have that older versions of Tiled did not serialize
            if (xTileset.Attribute("tilecount") == null || xTileset.Attribute("columns") == null)
            {
                m_Importer.ReportError("Old file format detected. You must save this file with a newer verion of Tiled.");
                return false;
            }

            ProcessAttributes(xTileset);
            BuildTileset(xTileset);
            ProcessTileElements(xTileset);

            return true;
        }

        private void ProcessAttributes(XElement xTileset)
        {
            m_TilesetScript.name = xTileset.GetAttributeAs<string>("name");
            m_TilesetScript.m_TileWidth = xTileset.GetAttributeAs<int>("tilewidth");
            m_TilesetScript.m_TileHeight = xTileset.GetAttributeAs<int>("tileheight");
            m_TilesetScript.m_Spacing = xTileset.GetAttributeAs<int>("spacing");
            m_TilesetScript.m_Margin = xTileset.GetAttributeAs<int>("margin");
            m_TilesetScript.m_TileCount = xTileset.GetAttributeAs<int>("tilecount");
            m_TilesetScript.m_TileColumns = xTileset.GetAttributeAs<int>("columns");

            var xTileOffset = xTileset.Element("tileoffset");
            if (xTileOffset != null)
            {
                var x = xTileOffset.GetAttributeAs<float>("x", 0.0f);
                var y = xTileOffset.GetAttributeAs<float>("y", 0.0f);
                m_TilesetScript.m_TileOffset = new Vector2(x, y);
            }

            m_TilesetScript.m_CustomProperties = CustomPropertyLoader.LoadCustomPropertyList(xTileset.Element("properties"));
        }

        private void BuildTileset(XElement xTileset)
        {
            // Build the initial database of tiles and the image components that make them
            // There are two ways that our collection of tiles can be created from images
            // 1) From one image broken down into parts (many tiles in one image)
            // 2) From a collection of images (one tile per image)

            var atlas = new AtlasBuilder(m_Importer, m_UseSpriteAtlas, (int)m_AtlasWidth, (int)m_AtlasHeight, m_TilesetScript);

            if (xTileset.Element("image") != null)
            {
                BuildTilesetFromImage(xTileset, atlas);
            }
            else
            {
                BuildTilesetFromCollection(xTileset, atlas);
            }

            // We're done collecting all the tile data. Build our atlas.
            // (Note that we call build even if we are not using texture atlases)
            atlas.Build();
        }

        private void BuildTilesetFromImage(XElement xTileset, AtlasBuilder atlas)
        {
            m_TilesetScript.m_IsImageCollection = false;

            XElement xImage = xTileset.Element("image");
            string textureAssetPath = xImage.GetAttributeAs<string>("source");
            int textureHeight = xImage.GetAttributeAs<int>("height");

            // Load the texture. We will make sprites and tiles out of this image.
            var tex2d = m_Importer.RequestAssetAtPath<Texture2D>(textureAssetPath);
            if (tex2d == null)
            {
                // Texture was not found so report the error to the importer UI and bail
                m_Importer.ReportError("Missing texture asset: {0}", textureAssetPath);
                return;
            }

            for (int i = 0; i < m_TilesetScript.m_TileCount; i++)
            {
                // Get grid x,y coords
                int x = i % m_TilesetScript.m_TileColumns;
                int y = i / m_TilesetScript.m_TileColumns;

                // Get x source on texture
                int srcx = x * m_TilesetScript.m_TileWidth;
                srcx += x * m_TilesetScript.m_Spacing;
                srcx += m_TilesetScript.m_Margin;

                // Get y source on texture
                int srcy = y * m_TilesetScript.m_TileHeight;
                srcy += y * m_TilesetScript.m_Spacing;
                srcy += m_TilesetScript.m_Margin;

                // In Tiled, texture origin is the top-left. However, in Unity the origin is bottom-left.
                srcy = (textureHeight - srcy) - m_TilesetScript.m_TileHeight;

                // Add the tile to our atlas
                Rect rcSource = new Rect(srcx, srcy, m_TilesetScript.m_TileWidth, m_TilesetScript.m_TileHeight);
                atlas.AddTile(i, tex2d, rcSource);
            }
        }

        private void BuildTilesetFromCollection(XElement xTileset, AtlasBuilder atlas)
        {
            m_TilesetScript.m_IsImageCollection = true;

            foreach (var xTile in xTileset.Elements("tile"))
            {
                int tileIndex = xTile.GetAttributeAs<int>("id");
                var xImage = xTile.Element("image");

                if (xImage != null)
                {
                    string textureAssetPath = xImage.GetAttributeAs<string>("source");

                    // Load the texture. We will make sprites and tiles out of this image.
                    var tex2d = m_Importer.RequestAssetAtPath<Texture2D>(textureAssetPath);
                    if (tex2d == null)
                    {
                        // Texture was not found yet so report the error to the importer UI and bail
                        m_Importer.ReportError("Missing texture asset for tile {0}: {1}", tileIndex, textureAssetPath);
                        return;
                    }

                    var rcSource = new Rect(0, 0, tex2d.width, tex2d.height);
                    atlas.AddTile(tileIndex, tex2d, rcSource);
                }
            }
        }

        private void ProcessTileElements(XElement xTileset)
        {
            // Additional processing on tiles
            foreach (var xTile in xTileset.Elements("tile"))
            {
                ProcessTileElement(xTile);
            }
        }

        private void ProcessTileElement(XElement xTile)
        {
            int index = xTile.GetAttributeAs<int>("id", -1);

            SuperTile tile;
            if (m_TilesetScript.TryGetTile(index, out tile))
            {
                // A tile may have a type associated with it
                tile.m_Type = xTile.GetAttributeAs("type", "");

                // Tiles can have custom properties (and properties inherited from their Type)
                tile.m_CustomProperties = CustomPropertyLoader.LoadCustomPropertyList(xTile.Element("properties"));
                tile.m_CustomProperties.AddPropertiesFromType(tile.m_Type, m_Importer.SuperImportContext);

                // Does the tile have any animation data?
                var xAnimation = xTile.Element("animation");
                if (xAnimation != null)
                {
                    ProcessAnimationElement(tile, xAnimation);
                }

                // Does the tile have an object group?
                var xObjectGroup = xTile.Element("objectgroup");
                if (xObjectGroup != null && xObjectGroup.HasElements)
                {
                    ProcessObjectGroupElement(tile, xObjectGroup);
                }
            }
        }

        private void ProcessAnimationElement(SuperTile tile, XElement xAnimation)
        {
            var fps = m_Importer.SuperImportContext.Settings.AnimationFramerate;
            var animations = new AnimationBuilder(fps);

            var frameSprites = xAnimation.Elements("frame").Select(f => f.GetAttributeAs<int>("tileid")).Select(index => m_TilesetScript.m_Tiles[index].m_Sprite).ToArray();
            var frameDurations = xAnimation.Elements("frame").Select(f => f.GetAttributeAs<int>("duration")).Select(ms => ms / 1000.0f).ToArray();

            for (int i = 0; i < frameSprites.Length; i++)
            {
                var sprite = frameSprites[i];
                var duration = frameDurations[i];
                animations.AddFrames(sprite, duration);
            }

            tile.m_AnimationSprites = animations.Sprites.ToArray();
        }

        private void ProcessObjectGroupElement(SuperTile tile, XElement xObjectGroup)
        {
            // Object groups on tiles come from the Tiled Collision Editor
            if (xObjectGroup.Elements().Any())
            {
                // We'll be adding collision objects to our tile
                tile.m_CollisionObjects = new List<CollisionObject>();

                foreach (var xObject in xObjectGroup.Elements("object"))
                {
                    var collision = new CollisionObject();

                    collision.m_ObjectId = xObject.GetAttributeAs("id", 0);
                    collision.m_ObjectName = xObject.GetAttributeAs("name", string.Format("Object_{0}", collision.m_ObjectId));
                    collision.m_ObjectType = xObject.GetAttributeAs("type", "");
                    collision.m_Position.x = xObject.GetAttributeAs("x", 0.0f);
                    collision.m_Position.y = xObject.GetAttributeAs("y", 0.0f);
                    collision.m_Size.x = xObject.GetAttributeAs("width", 0.0f);
                    collision.m_Size.y = xObject.GetAttributeAs("height", 0.0f);
                    collision.m_Rotation = xObject.GetAttributeAs("rotation", 0.0f);

                    // Are there any properties on the collision object?
                    collision.m_CustomProperties = CustomPropertyLoader.LoadCustomPropertyList(xObject.Element("properties"));
                    collision.m_CustomProperties.AddPropertiesFromType(collision.m_ObjectType, m_Importer.SuperImportContext);

                    var xPolygon = xObject.Element("polygon");
                    var xPolyline = xObject.Element("polyline");

                    if (xPolygon != null)
                    {
                        // Get points and make sure they are CCW
                        var points = xPolygon.GetAttributeAsVector2Array("points");

                        if (PolygonUtils.SumOverEdges(points) < 0)
                        {
                            points = points.Reverse().ToArray();
                        }

                        collision.MakePointsFromPolygon(points);
                    }
                    else if (xPolyline != null)
                    {
                        // Get points and make sure they are CCW
                        var points = xPolyline.GetAttributeAsVector2Array("points");

                        if (PolygonUtils.SumOverEdges(points) < 0)
                        {
                            points = points.Reverse().ToArray();
                        }

                        collision.MakePointsFromPolyline(points);
                    }
                    else if (xObject.Element("ellipse") != null)
                    {
                        if (collision.m_Size.x == 0)
                        {
                            m_Importer.ReportError("Invalid ellipse object Id '{0}' in tileset '{1}' has zero width", collision.m_ObjectId, m_TilesetScript.name);
                        }
                        else if (collision.m_Size.y == 0)
                        {
                            m_Importer.ReportError("Invalid ellipse object Id '{0}' in tileset '{1}' has zero height", collision.m_ObjectId, m_TilesetScript.name);
                        }
                        else
                        {
                            collision.MakePointsFromEllipse(m_Importer.SuperImportContext.Settings.EdgesPerEllipse);
                        }
                    }
                    else
                    {
                        // By default, objects are rectangles
                        if (collision.m_Size.x == 0)
                        {
                            m_Importer.ReportError("Invalid rectangle object Id '{0}' in tileset '{1}' has zero width", collision.m_ObjectId, m_TilesetScript.name);
                        }
                        else if (collision.m_Size.y == 0)
                        {
                            m_Importer.ReportError("Invalid rectangle object Id '{0}' in tileset '{1}' has zero height", collision.m_ObjectId, m_TilesetScript.name);
                        }
                        else
                        {
                            collision.MakePointsFromRectangle();
                        }
                    }

                    // Do not add if there are no points
                    if (!collision.Points.IsEmpty())
                    {
                        AssignCollisionObjectProperties(collision, tile);
                        tile.m_CollisionObjects.Add(collision);
                    }
                }
            }
        }

        private void AssignCollisionObjectProperties(CollisionObject collision, SuperTile tile)
        {
            // Check properties for layer name
            var layerProperty = GetCollisionOrTileOrTilesetProperty(collision, tile, "unity:layer");
            if (layerProperty != null)
            {
                // Explicit request to assign a layer to a collision. Report errors if the layer is missing.
                m_Importer.CheckLayerName(layerProperty.m_Value);
                collision.m_PhysicsLayer = layerProperty.m_Value;
            }
            else if (!string.IsNullOrEmpty(collision.m_ObjectName))
            {
                // Try to go off of object name but layer must exist (and we don't check for it)
                if (LayerMask.NameToLayer(collision.m_ObjectName) != -1)
                {
                    collision.m_PhysicsLayer = collision.m_ObjectName;
                }
            }

            if (string.IsNullOrEmpty(collision.m_PhysicsLayer))
            {
                collision.m_PhysicsLayer = "Default";
            }

            // Check properties for trigger setting
            var triggerProperty = GetCollisionOrTileOrTilesetProperty(collision, tile, "unity:isTrigger");
            if (triggerProperty != null)
            {
                collision.m_IsTrigger = triggerProperty.GetValueAsBool();
            }
        }

        private CustomProperty GetCollisionOrTileOrTilesetProperty(CollisionObject collision, SuperTile tile, string propertyName)
        {
            CustomProperty property;

            // Get property off the collision object
            if (collision.m_CustomProperties.TryGetProperty(propertyName, out property))
            {
                return property;
            }

            // Get property off the tile
            if (tile.m_CustomProperties.TryGetProperty(propertyName, out property))
            {
                return property;
            }

            // Inherit property from the tileset
            if (m_TilesetScript.m_CustomProperties.TryGetProperty(propertyName, out property))
            {
                return property;
            }

            return null;
        }
    }
}
