using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity.Editor
{
    [ScriptedImporter(ImporterConstants.MapVersion, ImporterConstants.MapExtension, ImporterConstants.MapImportOrder)]
    public partial class TmxAssetImporter : TiledAssetImporter
    {
        public const string TilesAsObjectsSerializedName = nameof(m_TilesAsObjects);
        public const string SortingModeSerializedName = nameof(m_SortingMode);
        public const string CustomImporterClassNameSerializedName = nameof(m_CustomImporterClassName);

        private SuperMap m_MapComponent;
        private Grid m_GridComponent;

        private GlobalTileDatabase m_GlobalTileDatabase;
        private Dictionary<uint, TilePolygonCollection> m_TilePolygonDatabase;
        private int m_NextTileAsObjectId;

        private Dictionary<int, GameObject> m_ObjectsById;

        [SerializeField]
        private bool m_TilesAsObjects = false;
        public bool TilesAsObjects => m_TilesAsObjects;

        [SerializeField]
        private SortingMode m_SortingMode = SortingMode.Stacked;
        public SortingMode SortingMode => m_SortingMode;

        [SerializeField]
        private bool m_IsIsometric = false;
        public bool IsIsometric => m_IsIsometric;

        [SerializeField]
        private string m_CustomImporterClassName = string.Empty;

        [SerializeField]
        private List<SuperTileset> m_InternalTilesets;

        /// Returns the game object corresponding to a Tiled object ID.
        public GameObject GetObject(int targetId)
        {
            if (m_ObjectsById.TryGetValue(targetId, out var go))
            {
                return go;
            }

            return null;
        }

        protected override void InternalOnImportAsset()
        {
            base.InternalOnImportAsset();
            ImporterVersion = ImporterConstants.MapVersion;
            AddSuperAsset<SuperAssetMap>();

            XDocument doc = XDocument.Load(assetPath);
            if (doc != null)
            {
                // Early out if Zstd compression is used. This simply isn't supported by Unity.
                if (doc.Descendants("data").Where(x => ((string)x.Attribute("compression")) == "zstd").Count() > 0)
                {
                    ReportGenericError("Unity does not support Zstandard compression.\nSelect a different 'Tile Layer Format' in your map settings in Tiled and resave.");
                    return;
                }

                var xMap = doc.Element("map");
                ProcessMap(xMap);
            }

            DoPrefabReplacements();
            DoCustomImporting();

            // Were any import errors captured along the way?
            m_MapComponent.m_ImportErrors = ImportErrors;
        }

        private void ProcessMap(XElement xMap)
        {
            Assert.IsNotNull(xMap);
            Assert.IsNull(m_MapComponent);
            Assert.IsNull(m_GlobalTileDatabase);

            m_TilePolygonDatabase = new Dictionary<uint, TilePolygonCollection>();
            RendererSorter.SortingMode = m_SortingMode;

            // Create our map and fill it out
            bool success = true;
            success = success && PrepareMainObject();
            success = success && ProcessMapAttributes(xMap);
            success = success && ProcessGridObject(xMap);
            success = success && ProcessTilesetElements(xMap);

            if (success)
            {
                // Custom properties need to be in place before we process the map layers
                AddSuperCustomProperties(m_MapComponent.gameObject, xMap.Element("properties"));

                using (SuperImportContext.BeginIsTriggerOverride(m_MapComponent.gameObject))
                {
                    // Add layers to our grid object
                    ProcessMapLayers(m_GridComponent.gameObject, xMap);
                    PostProccessMapLayers(m_GridComponent.gameObject);
                }
            }
        }

        // The map object is our Main Asset - the prefab that is created in our scene when dragged into the hierarchy
        private bool PrepareMainObject()
        {
            var icon = SuperIcons.instance.m_TmxIcon;

            var goGrid = new GameObject("_MapMainObject");
            SuperImportContext.AddObjectToAsset("_MapPrfab", goGrid, icon);
            SuperImportContext.SetMainObject(goGrid);
            m_MapComponent = goGrid.AddComponent<SuperMap>();

            return true;
        }

        private bool ProcessMapAttributes(XElement xMap)
        {
            m_MapComponent.name = Path.GetFileNameWithoutExtension(this.assetPath);
            m_MapComponent.m_Version = xMap.GetAttributeAs<string>("version");
            m_MapComponent.m_TiledVersion = xMap.GetAttributeAs<string>("tiledversion");

            m_MapComponent.m_Orientation = xMap.GetAttributeAs<MapOrientation>("orientation");
            m_MapComponent.m_RenderOrder = xMap.GetAttributeAs<MapRenderOrder>("renderorder");

            m_MapComponent.m_Width = xMap.GetAttributeAs<int>("width");
            m_MapComponent.m_Height = xMap.GetAttributeAs<int>("height");

            m_MapComponent.m_TileWidth = xMap.GetAttributeAs<int>("tilewidth");
            m_MapComponent.m_TileHeight = xMap.GetAttributeAs<int>("tileheight");

            m_MapComponent.m_HexSideLength = xMap.GetAttributeAs<int>("hexsidelength");
            m_MapComponent.m_StaggerAxis = xMap.GetAttributeAs<StaggerAxis>("staggeraxis");
            m_MapComponent.m_StaggerIndex = xMap.GetAttributeAs<StaggerIndex>("staggerindex");

            m_MapComponent.m_Infinite = xMap.GetAttributeAs<bool>("infinite");
            m_MapComponent.m_BackgroundColor = xMap.GetAttributeAsColor("backgroundcolor", NamedColors.Gray);
            m_MapComponent.m_NextObjectId = xMap.GetAttributeAs<int>("nextobjectid");

            m_IsIsometric = m_MapComponent.m_Orientation == MapOrientation.Isometric;
            m_NextTileAsObjectId = m_MapComponent.m_NextObjectId;

            return true;
        }

        private bool ProcessGridObject(XElement xMap)
        {
            // Add the grid to the map
            var goGrid = new GameObject("Grid");
            goGrid.transform.SetParent(m_MapComponent.gameObject.transform);

            m_GridComponent = goGrid.AddComponent<Grid>();

            // Grid cell size always has a z-value of 1 so that we can use custom axis sorting
            float sx = SuperImportContext.MakeScalar(m_MapComponent.m_TileWidth);
            float sy = SuperImportContext.MakeScalar(m_MapComponent.m_TileHeight);
            m_GridComponent.cellSize = new Vector3(sx, sy, 1);
            Vector3 tilemapOffset = new Vector3(0, 0, 0);

            switch (m_MapComponent.m_Orientation)
            {
                case MapOrientation.Isometric:
                    m_GridComponent.cellLayout = GridLayout.CellLayout.Isometric;
                    tilemapOffset = new Vector3(-sx * 0.5f, -sy, 0);
                    break;

                case MapOrientation.Staggered:
                    m_GridComponent.cellLayout = GridLayout.CellLayout.Isometric;

                    if (m_MapComponent.m_StaggerAxis == StaggerAxis.Y)
                    {
                        if (m_MapComponent.m_StaggerIndex == StaggerIndex.Odd)
                        {
                            // Y - Odd
                            tilemapOffset = new Vector3(0, -sy, 0);
                        }
                        else
                        {
                            // Y-Even
                            tilemapOffset = new Vector3(sx * 0.5f, -sy, 0);
                        }
                    }
                    else if (m_MapComponent.m_StaggerAxis == StaggerAxis.X)
                    {
                        // X-Ood
                        if (m_MapComponent.m_StaggerIndex == StaggerIndex.Odd)
                        {
                            tilemapOffset = new Vector3(0, -sy, 0);
                        }
                        else
                        {
                            // X-Even
                            tilemapOffset = new Vector3(0, -sy * 1.5f, 0);
                        }
                    }
                    break;

                case MapOrientation.Hexagonal:
                    if (m_MapComponent.m_StaggerAxis == StaggerAxis.Y)
                    {
                        // Pointy-top hex maps
                        m_GridComponent.cellLayout = GridLayout.CellLayout.Hexagon;
                        m_GridComponent.cellSwizzle = GridLayout.CellSwizzle.XYZ;

                        if (m_MapComponent.m_StaggerIndex == StaggerIndex.Odd)
                        {
                            // Y-Odd
                            tilemapOffset = new Vector3(0, -sy, 0);
                        }
                        else
                        {
                            // Y-Even
                            tilemapOffset = new Vector3(0, -sy * 0.25f, 0);
                        }
                    }
                    else if (m_MapComponent.m_StaggerAxis == StaggerAxis.X)
                    {
                        // Flat-top hex maps. Reverse x and y on size.
                        m_GridComponent.cellLayout = GridLayout.CellLayout.Hexagon;
                        m_GridComponent.cellSwizzle = GridLayout.CellSwizzle.YXZ;
                        m_GridComponent.cellSize = new Vector3(sy, sx, 1);

                        if (m_MapComponent.m_StaggerIndex == StaggerIndex.Odd)
                        {
                            // X-Odd
                            tilemapOffset = new Vector3(-sx * 0.75f, -sy * 1.5f, 0);
                        }
                        else
                        {
                            // X-Even
                            tilemapOffset = new Vector3(0, -sy * 1.5f, 0);
                        }
                    }
                    break;

                default:
                    m_GridComponent.cellLayout = GridLayout.CellLayout.Rectangle;
                    tilemapOffset = new Vector3(0, -sy, 0);
                    break;
            }

            SuperImportContext.TilemapOffset = tilemapOffset;

            return true;
        }

        private bool ProcessTilesetElements(XElement xMap)
        {
            Assert.IsNull(m_GlobalTileDatabase);

            bool success = true;

            // Our tile database will be fed with tiles from each referenced tileset
            m_GlobalTileDatabase = new GlobalTileDatabase();
            m_InternalTilesets = new List<SuperTileset>();

            foreach (var xTileset in xMap.Elements("tileset"))
            {
                if (xTileset.Attribute("source") != null)
                {
                    success = success && ProcessTilesetElementExternal(xTileset);
                }
                else
                {
                    success = success && ProcessTilesetElementInternal(xTileset);
                }
            }

            return success;
        }

        private bool ProcessTilesetElementExternal(XElement xTileset)
        {
            Assert.IsNotNull(xTileset);
            Assert.IsNotNull(m_GlobalTileDatabase);

            var firstId = xTileset.GetAttributeAs<int>("firstgid");
            var source = xTileset.GetAttributeAs<string>("source");

            // JSON customized assets are not supported as Unity has the *.json extension reserved
            if (string.Equals(Path.GetExtension(source), ".json", StringComparison.OrdinalIgnoreCase))
            {
                ReportGenericError($"JSON tilesets are not supported by Unity. Use TSX files instead. Tileset: {source}");
                return false;
            }

            // Is the source tileset a "special" tileset embedded into Tiled?
            if (source == ":/automap-tiles.tsx")
            {
                // automap-tiles.tsx currently has 5 tiles used by automapping. These are ignored in Unity.
                m_GlobalTileDatabase.RegisterIgnorableTile(firstId + 0);
                m_GlobalTileDatabase.RegisterIgnorableTile(firstId + 1);
                m_GlobalTileDatabase.RegisterIgnorableTile(firstId + 2);
                m_GlobalTileDatabase.RegisterIgnorableTile(firstId + 3);
                m_GlobalTileDatabase.RegisterIgnorableTile(firstId + 4);
                return true;
            }

            // Load the tileset and process the tiles inside
            var tileset = RequestDependencyAssetAtPath<SuperTileset>(source);
            if (tileset == null)
            {
                // Tileset is missing or was not imported properly
                return false;
            }
            else
            {
                // Register all the tiles with the tile database for this map
                m_GlobalTileDatabase.RegisterTileset(firstId, tileset);
            }

            return true;
        }

        private bool ProcessTilesetElementInternal(XElement xTileset)
        {
            var firstId = xTileset.GetAttributeAs<int>("firstgid");
            var name = xTileset.GetAttributeAs<string>("name");

            var tileset = ScriptableObject.CreateInstance<SuperTileset>();
            tileset.m_IsInternal = true;
            tileset.name = name;
            tileset.m_PixelsPerUnit = PixelsPerUnit;
            m_InternalTilesets.Add(tileset);

            string assetName = string.Format("_TilesetScriptObjectInternal_{0}", m_InternalTilesets.Count);
            SuperImportContext.AddObjectToAsset(assetName, tileset);

            var loader = new TilesetLoader(tileset, this, firstId);
            if (loader.LoadFromXml(xTileset))
            {
                m_GlobalTileDatabase.RegisterTileset(firstId, tileset);
                return true;
            }

            return false;
        }

        private void ProcessMapLayers(GameObject goParent, XElement xMap)
        {
            // Note that this method is re-entrant due to group layers
            foreach (XElement xNode in xMap.Elements())
            {
                if (!xNode.GetAttributeAs<bool>("visible", true))
                {
                    continue;
                }

                LayerIgnoreMode ignoreMode = xNode.GetPropertyAttributeAs(StringConstants.Unity_Ignore, SuperImportContext.LayerIgnoreMode);
                if (ignoreMode == LayerIgnoreMode.True)
                {
                    continue;
                }

                using (SuperImportContext.BeginLayerIgnoreMode(ignoreMode))
                {
                    if (xNode.Name == "layer")
                    {
                        ProcessTileLayer(goParent, xNode);
                    }
                    else if (xNode.Name == "group")
                    {
                        ProcessGroupLayer(goParent, xNode);
                    }
                    else if (xNode.Name == "objectgroup")
                    {
                        ProcessObjectLayer(goParent, xNode);
                    }
                    else if (xNode.Name == "imagelayer")
                    {
                        ProcessImageLayer(goParent, xNode);
                    }
                }
            }
        }

        private void PostProccessMapLayers(GameObject goParent)
        {
            foreach (var layer in goParent.GetComponentsInChildren<SuperLayer>())
            {
                layer.SetWorldPosition(m_MapComponent, SuperImportContext);
            }

            // Refresh all our tilemaps so that needless prefab instance changes don't appear
            foreach (var tilemap in goParent.GetComponentsInChildren<Tilemap>())
            {
                tilemap.RefreshAllTiles();
                CalculateChunkCullingBounds(tilemap);
            }
        }

        private static void CalculateChunkCullingBounds(Tilemap tilemap)
        {
            // Each tilemap gameobject should have a tilemap renderer
            // Set the chunk culling bounds of the renderer using the size of the largest sprite in the tilemap
            var tilemapRenderer = tilemap.gameObject.GetComponent<TilemapRenderer>();
            if (tilemapRenderer != null)
            {
                tilemapRenderer.detectChunkCullingBounds = TilemapRenderer.DetectChunkCullingBounds.Manual;
                float maxWidth = 0;
                float maxHeight = 0;

                var allTiles = tilemap.GetTilesBlock(tilemap.cellBounds);
                foreach (var tile in allTiles.OfType<SuperTile>())
                {
                    maxWidth = Mathf.Max(tile.m_Sprite.bounds.size.x, maxWidth);
                    maxHeight = Mathf.Max(tile.m_Sprite.bounds.size.y, maxHeight);

                    // Look out for animated tiles
                    if (!tile.m_AnimationSprites.IsEmpty())
                    {
                        foreach (var sprite in tile.m_AnimationSprites)
                        {
                            maxWidth = Mathf.Max(sprite.bounds.size.x, maxWidth);
                            maxHeight = Mathf.Max(sprite.bounds.size.y, maxHeight);
                        }
                    }
                }

                tilemapRenderer.chunkCullingBounds = new Vector3(maxWidth, maxHeight, 0);
            }
        }

        private void DoPrefabReplacements()
        {
            // Should any of our objects (from Tiled) be replaced by instantiated prefabs?
            var supers = m_MapComponent.GetComponentsInChildren<SuperObject>();
            m_ObjectsById = supers.ToDictionary(so => so.m_Id, so => so.gameObject);
            var goToDestroy = new List<GameObject>();

            foreach (var so in supers)
            {
                var prefab = ST2USettings.instance.GetPrefabReplacement(so.m_Type);
                if (prefab != null)
                {
                    // Replace the super object with the instantiated prefab
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    instance.transform.SetParent(so.transform.parent);
                    instance.transform.position = so.transform.position + prefab.transform.localPosition;
                    instance.transform.rotation = so.transform.rotation;

                    // Keep the name from Tiled.
                    instance.name = so.gameObject.name;

                    // Update bookkeeping for later custom property replacement.
                    goToDestroy.Add(so.gameObject);
                    m_ObjectsById[so.m_Id] = instance;
                }
            }

            // Now that all the replacements have been instantiated, apply custom properties
            // where object references can now also point to the new replacement instances.
            foreach (var so in supers)
            {
                // Apply custom properties as messages to the instanced prefab
                var props = so.GetComponent<SuperCustomProperties>();
                if (props != null)
                {
                    foreach (var p in props.m_Properties)
                    {
                        m_ObjectsById[so.m_Id].BroadcastProperty(p, m_ObjectsById, ReportGenericError);
                    }
                }
            }

            // Finally, destroy replaced game objects.
            foreach (var go in goToDestroy)
            {
                DestroyImmediate(go);
            }
        }

        private void DoCustomImporting()
        {
            ApplyUserImporter();
            ApplyAutoImporters();
        }

        private void ApplyUserImporter()
        {
            if (!string.IsNullOrEmpty(m_CustomImporterClassName))
            {
                var type = AppDomain.CurrentDomain.GetTypeFromName(m_CustomImporterClassName);

                if (type == null)
                {
                    ReportGenericError($"Custom Importer error. Class type '{m_CustomImporterClassName}' is missing. Error importing '{assetPath}'");
                    return;
                }

                RunCustomImporterType(type);
            }
        }

        private void ApplyAutoImporters()
        {
            foreach (var type in AutoCustomTmxImporterAttribute.GetOrderedAutoImportersTypes())
            {
                RunCustomImporterType(type);
            }
        }

        private void RunCustomImporterType(Type type)
        {
            // Instantiate a custom importer class for specialized projects to use
            CustomTmxImporter customImporter;
            try
            {
                customImporter = Activator.CreateInstance(type) as CustomTmxImporter;
            }
            catch (Exception e)
            {
                ReportGenericError($"Error creating custom importer class. Message = '{e.Message}'\n{e.StackTrace}");
                return;
            }

            try
            {
                var args = new TmxAssetImportedArgs();
                args.AssetImporter = this;
                args.ImportedSuperMap = m_MapComponent;

                customImporter.TmxAssetImported(args);
            }
            catch (CustomImporterException cie)
            {
                ReportGenericError($"Custom Importer error: \n  Importer: {customImporter.GetType().Name}\n  Message: {cie.Message}");
                Debug.LogErrorFormat("Custom Importer ({0}) exception: {1}", customImporter.GetType().Name, cie.Message);
            }
            catch (Exception e)
            {
                ReportGenericError($"Custom importer '{customImporter.GetType().Name}' threw an exception. Message = '{e.Message}', Stack:\n{e.StackTrace}");
                Debug.LogErrorFormat("Custom importer general exception: {0}", e.Message);
            }
        }
    }
}