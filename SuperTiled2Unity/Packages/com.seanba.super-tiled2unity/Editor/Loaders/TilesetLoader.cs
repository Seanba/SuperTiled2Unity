using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // Tilesets can be stand-alone TSX files (preferred) or embedded in Tiled Map Editor files (TMX)
    // This helper loader class gives us the flexibility we need to load tileset data
    public class TilesetLoader
    {
        private readonly SuperTileset m_SuperTileset;
        private readonly TiledAssetImporter m_Importer;
        private readonly int m_InternalId;

        public const string SpriteNameRoot = "st2u_";

        // Make this an ugly name that shouldn't collide with sprites named by the user
        internal static string RectToSpriteName(Rect rect)
        {
            return $"{SpriteNameRoot}x{rect.x}y{rect.y}-w{rect.width}h{rect.height}";
        }

        public TilesetLoader(SuperTileset tileset, TiledAssetImporter importer, int internalId)
        {
            m_SuperTileset = tileset;
            m_Importer = importer;
            m_InternalId = internalId;
        }

        public bool LoadFromXml(XElement xTileset)
        {
            // There are attributes we *must* have that older versions of Tiled did not serialize
            if (xTileset.Attribute("tilecount") == null || xTileset.Attribute("columns") == null)
            {
                m_Importer.ReportGenericError($"Old Tiled file format detected. Try resaving file '{m_Importer.assetPath}' with a newer verion of Tiled (At least 0.15).");
                return false;
            }

            ProcessAttributes(xTileset);
            BuildTileset(xTileset);
            ProcessTileElements(xTileset);
            return true;
        }

        private void ProcessAttributes(XElement xTileset)
        {
            m_SuperTileset.name = xTileset.GetAttributeAs<string>("name");
            m_SuperTileset.m_TileWidth = xTileset.GetAttributeAs<int>("tilewidth");
            m_SuperTileset.m_TileHeight = xTileset.GetAttributeAs<int>("tileheight");
            m_SuperTileset.m_Spacing = xTileset.GetAttributeAs<int>("spacing");
            m_SuperTileset.m_Margin = xTileset.GetAttributeAs<int>("margin");
            m_SuperTileset.m_TileCount = xTileset.GetAttributeAs<int>("tilecount");
            m_SuperTileset.m_TileColumns = xTileset.GetAttributeAs<int>("columns");
            m_SuperTileset.m_ObjectAlignment = xTileset.GetAttributeAs<ObjectAlignment>("objectalignment", ObjectAlignment.Unspecified);
            m_SuperTileset.m_TileRenderSize = xTileset.GetAttributeAs<TileRenderSize>("tilerendersize", TileRenderSize.Tile);
            m_SuperTileset.m_FillMode = xTileset.GetAttributeAs<FillMode>("fillmode", FillMode.Stretch);

            var xTileOffset = xTileset.Element("tileoffset");
            if (xTileOffset != null)
            {
                var x = xTileOffset.GetAttributeAs<float>("x", 0.0f);
                var y = xTileOffset.GetAttributeAs<float>("y", 0.0f);
                m_SuperTileset.m_TileOffset = new Vector2(x, y);
            }

            var xGrid = xTileset.Element("grid");
            if (xGrid != null)
            {
                m_SuperTileset.m_GridOrientation = xGrid.GetAttributeAs<GridOrientation>("orientation");

                var w = xGrid.GetAttributeAs<float>("width", 0.0f);
                var h = xGrid.GetAttributeAs<float>("height", 0.0f);
                m_SuperTileset.m_GridSize = new Vector2(w, h);
            }

            m_SuperTileset.m_CustomProperties = CustomPropertyLoader.LoadCustomPropertyList(xTileset.Element("properties"));
        }

        private void BuildTileset(XElement xTileset)
        {
            // Build the initial database of tiles and the image components that make them
            // There are two ways that our collection of tiles can be created from images
            // 1) From one image broken down into parts (many tiles in one image)
            // 2) From a collection of images (one tile per image)
            if (xTileset.Element("image") != null)
            {
                BuildTilesetFromImage(xTileset);
            }
            else
            {
                BuildTilesetFromCollection(xTileset);
            }
        }

        private void BuildTilesetFromImage(XElement xTileset)
        {
            m_SuperTileset.m_IsImageCollection = false;

            XElement xImage = xTileset.Element("image");
            string textureLocalPath = xImage.GetAttributeAs<string>("source");
            int textureWidth = xImage.GetAttributeAs<int>("width");
            int textureHeight = xImage.GetAttributeAs<int>("height");

            bool forceErrorTiles = false;

            // Load the texture. We will make sprites and tiles out of this image.
            var tex2d = m_Importer.RequestDependencyAssetAtPath<Texture2D>(textureLocalPath);
            var textureAssetPath = string.Empty;
            var sprites = new Dictionary<(Rect, Vector2), Sprite>();

            if (tex2d == null)
            {
                m_Importer.ReportMissingDependency(textureLocalPath);
                forceErrorTiles = true;
            }
            else
            {
                textureAssetPath = AssetDatabase.GetAssetPath(tex2d);

                if (AssetImporter.GetAtPath(textureAssetPath) is TextureImporter textureImporter)
                {
                    textureImporter.GetSourceTextureWidthAndHeight(out int sourceWidth, out int sourceHeight);
                    if (tex2d.width != sourceWidth || tex2d.height != sourceHeight)
                    {
                        forceErrorTiles = true;
                        m_Importer.ReportWrongTextureSize(textureAssetPath, sourceWidth, sourceHeight, tex2d.width, tex2d.height);
                    }
                    else if (textureImporter.spritePixelsPerUnit != m_SuperTileset.m_PixelsPerUnit)
                    {
                        forceErrorTiles = true;
                        m_Importer.ReportWrongPixelsPerUnit(textureAssetPath, textureImporter.spritePixelsPerUnit, m_SuperTileset.m_PixelsPerUnit);
                    }
                }

                if (!forceErrorTiles)
                {
                    sprites = AssetDatabase.LoadAllAssetsAtPath(textureAssetPath).OfType<Sprite>().SafeToDictionary(s => (s.rect, s.pivot), s => s);
                }
            }

            for (int i = 0; i < m_SuperTileset.m_TileCount; i++)
            {
                // Get grid x,y coords
                int x = i % m_SuperTileset.m_TileColumns;
                int y = i / m_SuperTileset.m_TileColumns;

                int tileWidth = m_SuperTileset.m_TileWidth;
                int tileHeight = m_SuperTileset.m_TileHeight;

                // Get x source on texture
                int srcx = x * tileWidth;
                srcx += x * m_SuperTileset.m_Spacing;
                srcx += m_SuperTileset.m_Margin;

                // Get y source on texture
                int srcy = y * tileHeight;
                srcy += y * m_SuperTileset.m_Spacing;
                srcy += m_SuperTileset.m_Margin;

                // In Tiled, texture origin is the top-left. However, in Unity the origin is bottom-left.
                srcy = (textureHeight - srcy) - tileHeight;

                if (forceErrorTiles || !TryAddTile(i, srcx, srcy, tileWidth, tileHeight, sprites))
                {
                    if (!string.IsNullOrEmpty(textureAssetPath))
                    {
                        m_Importer.ReportMissingSprite(textureAssetPath, i, srcx, srcy, tileWidth, tileHeight);
                    }

                    AddErrorTile(i, NamedColors.HotPink, tileWidth, tileHeight);
                }
            }
        }

        private void BuildTilesetFromCollection(XElement xTileset)
        {
            m_SuperTileset.m_IsImageCollection = true;

            foreach (var xTile in xTileset.Elements("tile"))
            {
                int tileIndex = xTile.GetAttributeAs<int>("id");
                var xImage = xTile.Element("image");

                if (xImage != null)
                {
                    string textureLocalPath = xImage.GetAttributeAs<string>("source");
                    int texture_w = xImage.GetAttributeAs<int>("width");
                    int texture_h = xImage.GetAttributeAs<int>("height");

                    bool forceErrorTiles = false;

                    var tex2d = m_Importer.RequestDependencyAssetAtPath<Texture2D>(textureLocalPath);
                    var textureAssetPath = string.Empty;
                    var sprites = new Dictionary<(Rect, Vector2), Sprite>();

                    // Load the texture. We will make sprites and tiles out of this image.
                    if (tex2d == null)
                    {
                        m_Importer.ReportMissingDependency(textureLocalPath);
                        forceErrorTiles = true;
                    }
                    else
                    {
                        textureAssetPath = AssetDatabase.GetAssetPath(tex2d);

                        // The pixels per unit of the sprites must match the pixels per unit of the tileset
                        sprites = AssetDatabase.LoadAllAssetsAtPath(textureAssetPath).OfType<Sprite>().SafeToDictionary(s => (s.rect, s.pivot), s => s);
                        if (sprites.Any())
                        {
                            var firstSprite = sprites.First().Value;
                            if (firstSprite.pixelsPerUnit != m_SuperTileset.m_PixelsPerUnit)
                            {
                                m_Importer.ReportWrongPixelsPerUnit(textureAssetPath, firstSprite.pixelsPerUnit, m_SuperTileset.m_PixelsPerUnit);
                                forceErrorTiles = true;
                            }
                        }
                    }

                    int tile_x = xTile.GetAttributeAs<int>("x", 0);
                    int tile_y = xTile.GetAttributeAs<int>("y", 0);
                    int tile_w = xTile.GetAttributeAs<int>("width", texture_w);
                    int tile_h = xTile.GetAttributeAs<int>("height", texture_h);

                    // In Tiled, texture origin is the top-left. However, in Unity the origin is bottom-left.
                    if (tex2d != null)
                    {
                        tile_y = (tex2d.height - tile_y) - tile_h;
                    }

                    if (!forceErrorTiles && sprites.Count == 1)
                    {
                        if (AssetImporter.GetAtPath(textureAssetPath) is TextureImporter importer && importer.spriteImportMode == SpriteImportMode.Single)
                        {
                            var rect = new Rect(tile_x, tile_y, tile_w, tile_h);
                            if (!sprites.TryGetValue((rect, Vector2.zero), out Sprite tileSprite))
                            {
                                // Annoying special case where the source texture is imported as a single sprite
                                // This may be because the user wants to use the texture both as a native Unity sprite and as part of a Tiled tileset
                                // In this case we cannot have ST2U split up the texture into sprite rectangles
                                // We have to create a new sprite with the pivot and size we want and store it in the imported asset
                                tileSprite = Sprite.Create(tex2d, rect, Vector2.zero, importer.spritePixelsPerUnit);
                                tileSprite.name = $"Sprite.{Path.GetFileNameWithoutExtension(textureAssetPath)}";
                                tileSprite.hideFlags = HideFlags.HideInHierarchy;
                                string uniqueId = $"{tileSprite.name}.{tileIndex}.{m_InternalId}";
                                m_Importer.SuperImportContext.AddObjectToAsset(uniqueId, tileSprite);
                                sprites.Add((tileSprite.rect, tileSprite.pivot), tileSprite);
                            }
                        }
                    }

                    if (forceErrorTiles || !TryAddTile(tileIndex, tile_x, tile_y, tile_w, tile_h, sprites))
                    {
                        if (!string.IsNullOrEmpty(textureAssetPath))
                        {
                            m_Importer.ReportMissingSprite(textureAssetPath, tileIndex, tile_x, tile_y, tile_w, tile_h);
                        }

                        AddErrorTile(tileIndex, NamedColors.DeepPink, tile_w, tile_h);
                    }
                }
            }
        }

        private bool TryAddTile(int tileId, int x, int y, int width, int height, Dictionary<(Rect, Vector2), Sprite> sprites)
        {
            var rect = new Rect(x, y, width, height);
            if (sprites.TryGetValue((rect, Vector2.zero), out Sprite tileSprite))
            {
                // Create the tile that uses the sprite
                var tile = SuperTile.CreateSuperTile();
                tile.m_TileId = tileId;
                tile.name = tileSprite.name;
                tile.hideFlags = HideFlags.HideInHierarchy;
                tile.m_Sprite = tileSprite;
                tile.m_Width = rect.width;
                tile.m_Height = rect.height;
                tile.m_TileOffsetX = m_SuperTileset.m_TileOffset.x;
                tile.m_TileOffsetY = m_SuperTileset.m_TileOffset.y;
                tile.m_ObjectAlignment = m_SuperTileset.m_ObjectAlignment;
                tile.m_TileRenderSize = m_SuperTileset.m_TileRenderSize;
                tile.m_FillMode = m_SuperTileset.m_FillMode;

                if (m_Importer is TsxAssetImporter tsxAssetImporter)
                {
                    tile.m_ColliderType = tsxAssetImporter.m_ColliderType;
                }

                m_SuperTileset.m_Tiles.Add(tile);

                // The identifier for the tile *must* be unique amoung all other objects that are added to the same import context
                string uniqueId = $"Tile.{tileSprite.name}.{m_InternalId}.{tileId}";
                m_Importer.SuperImportContext.AddObjectToAsset(uniqueId, tile);

                return true;
            }

            return false;
        }

        private void AddErrorTile(int tileId, Color tint, int width, int height)
        {
            BadTileSpriteProvider.instance.CreateSpriteAndTile(tileId, tint, width, height, m_SuperTileset, out Sprite sprite, out SuperBadTile tile);

            sprite.hideFlags = HideFlags.HideInHierarchy;
            tile.hideFlags = HideFlags.HideInHierarchy;

            m_SuperTileset.m_Tiles.Add(tile);
            m_Importer.SuperImportContext.AddObjectToAsset(sprite.name, sprite);
            m_Importer.SuperImportContext.AddObjectToAsset(tile.name, tile);
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

            if (m_SuperTileset.TryGetTile(index, out SuperTile tile))
            {
                // A tile may have a class associated with it
                tile.m_Type = xTile.GetAttributeAs("class", "");

                // As of Tiled 1.9 types have been merged with classes
                // As a simple way to support both version we can fall back like this to the old < 1.9 way
                if (string.IsNullOrWhiteSpace(tile.m_Type))
                {
                    tile.m_Type = xTile.GetAttributeAs("type", "");
                }

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
            var fps = ST2USettings.instance.m_AnimationFramerate;
            var animations = new AnimationBuilder(fps);

            foreach (var xFrame in xAnimation.Elements("frame"))
            {
                var frameId = xFrame.GetAttributeAs<int>("tileid");
                var frameDuration = xFrame.GetAttributeAs<int>("duration") / 1000.0f;

                if (m_SuperTileset.TryGetTile(frameId, out SuperTile frame))
                {
                    animations.AddFrames(frame.m_Sprite, frameDuration);
                }
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

                    // Template may fill in a bunch of properties
                    m_Importer.ApplyTemplateToObject(xObject);

                    collision.m_ObjectId = xObject.GetAttributeAs("id", 0);
                    collision.m_ObjectName = xObject.GetAttributeAs("name", string.Format("Object_{0}", collision.m_ObjectId));

                    // As of Tiled 1.9 types have been merged with classes
                    // As a simple way to support both version we can fall back like this to the old < 1.9 way
                    collision.m_ObjectType = xObject.GetAttributeAs("class", "");
                    if (string.IsNullOrWhiteSpace(collision.m_ObjectType))
                    {
                        collision.m_ObjectType = xObject.GetAttributeAs("type", "");
                    }

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
                            m_Importer.ReportGenericError($"Invalid ellipse (Tile ID ='{tile.m_TileId}') in tileset '{m_SuperTileset.name}' has zero width");
                        }
                        else if (collision.m_Size.y == 0)
                        {
                            m_Importer.ReportGenericError($"Invalid ellipse (Tile ID ='{tile.m_TileId}') in tileset '{m_SuperTileset.name}' has zero height");
                        }
                        else
                        {
                            collision.MakePointsFromEllipse(m_Importer.EdgesPerEllipse);
                        }
                    }
                    else if (xObject.Element("point") != null)
                    {
                        collision.MakePoint();
                    }
                    else
                    {
                        // By default, objects are rectangles
                        if (collision.m_Size.x == 0)
                        {
                            m_Importer.ReportGenericError($"Invalid rectangle (Tile ID ='{tile.m_TileId}') in tileset '{m_SuperTileset.name}' has zero width");
                        }
                        else if (collision.m_Size.y == 0)
                        {
                            m_Importer.ReportGenericError($"Invalid rectangle (Tile ID ='{tile.m_TileId}') in tileset '{m_SuperTileset.name}' has zero height");
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

                        collision.RenderPoints(tile, m_SuperTileset.m_GridOrientation, m_SuperTileset.m_GridSize);
                        tile.m_CollisionObjects.Add(collision);
                    }
                }
            }
        }

        private void AssignCollisionObjectProperties(CollisionObject collision, SuperTile tile)
        {
            // Check properties for layer name
            var layerProperty = GetCollisionOrTileOrTilesetProperty(collision, tile, StringConstants.Unity_Layer);
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
            var triggerProperty = GetCollisionOrTileOrTilesetProperty(collision, tile, StringConstants.Unity_IsTrigger);
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
            if (m_SuperTileset.m_CustomProperties.TryGetProperty(propertyName, out property))
            {
                return property;
            }

            return null;
        }
    }
}
