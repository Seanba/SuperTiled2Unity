using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity.Editor
{
    public partial class TmxAssetImporter
    {
        private CollisionBuilder m_CurrentCollisionBuilder;

        private struct Chunk
        {
            public DataEncoding Encoding { get; set; }
            public DataCompression Compression { get; set; }

            public XElement XmlChunk { get; set; }

            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        private GameObject ProcessTileLayer(GameObject goParent, XElement xLayer)
        {
            Assert.IsNotNull(xLayer);
            Assert.IsNotNull(goParent);

            // Create the game object that contains the layer and add it to the grid parent
            var layerComponent = goParent.AddSuperLayerGameObject<SuperTileLayer>(new SuperTileLayerLoader(xLayer), SuperImportContext);

            // Add properties then sort the layer
            AddSuperCustomProperties(layerComponent.gameObject, xLayer.Element("properties"));
            m_LayerSorterHelper.SortNewLayer(layerComponent);

            // Process the data for the layer
            var xData = xLayer.Element("data");
            if (xData != null)
            {
                ProcessLayerData(layerComponent.gameObject, xData);
            }

            m_TileLayerCounter++;
            return layerComponent.gameObject;
        }

        private void ProcessLayerData(GameObject goLayer, XElement xData)
        {
            Assert.IsNotNull(goLayer);
            Assert.IsNotNull(goLayer.GetComponent<SuperTileLayer>());
            Assert.IsNotNull(xData);

            SuperTileLayer superComp = goLayer.GetComponent<SuperTileLayer>();

            var chunk = new Chunk();
            chunk.Encoding = xData.GetAttributeAs<DataEncoding>("encoding");
            chunk.Compression = xData.GetAttributeAs<DataCompression>("compression");

            // Are we reading in data in smaller chunks (for infinite maps) or one big chunk (the full map)
            var xChunks = xData.Elements("chunk");
            if (xChunks.Any())
            {
                foreach (var xChunk in xChunks)
                {
                    chunk.XmlChunk = xChunk;
                    chunk.X = xChunk.GetAttributeAs<int>("x");
                    chunk.Y = xChunk.GetAttributeAs<int>("y");
                    chunk.Width = xChunk.GetAttributeAs<int>("width");
                    chunk.Height = xChunk.GetAttributeAs<int>("height");

                    // Each chunk is a separate game object on the super layer
                    GameObject goChunk = new GameObject(string.Format("Chunk ({0},{1})", chunk.X, chunk.Y));
                    goLayer.AddChildWithUniqueName(goChunk);

                    // Position the chunk
                    Vector2 translate = m_MapComponent.MapCoordinatesToPositionPPU(chunk.X, chunk.Y);
                    goChunk.transform.localPosition = translate;

                    // Create the tilemap for the layer if needed
                    if (!m_TilesAsObjects)
                    {
                        GetOrAddTilemapComponent(goChunk, superComp);
                    }

                    ProcessLayerDataChunk(goChunk, chunk);
                }
            }
            else
            {
                // Regular maps only have one chunk with the Tilemap and TileRenderer being on the layer object

                // Add the tilemap components if needed
                if (!m_TilesAsObjects)
                {
                    GetOrAddTilemapComponent(goLayer, superComp);
                }

                // For regular maps the 'chunk' is the same as the layer data
                chunk.XmlChunk = xData;
                chunk.X = 0;
                chunk.Y = 0;
                chunk.Width = m_MapComponent.m_Width;
                chunk.Height = m_MapComponent.m_Height;

                ProcessLayerDataChunk(goLayer, chunk);
            }
        }

        private Tilemap GetOrAddTilemapComponent(GameObject go, SuperTileLayer layer)
        {
            // If we already have a Tilemap component up our ancesters then we are using a format that does not support separate tilemaps
            var parentTilemap = go.GetComponentInParent<Tilemap>();

            if (parentTilemap == null)
            {
                // Need tilemap data if we're going to have tilemap for flips and rotations
                go.AddComponent<TilemapData>();

                var tilemap = go.AddComponent<Tilemap>();
                tilemap.tileAnchor = m_MapComponent.GetTileAnchor();
                tilemap.animationFrameRate = SuperImportContext.Settings.AnimationFramerate;

                // Create the renderer for the layer
                var renderer = AddTilemapRendererComponent(go);
                AssignMaterial(renderer);

                if (layer != null)
                {
                    tilemap.color = new Color(1, 1, 1, layer.CalculateOpacity());
                    AssignSortingLayer(renderer, layer.m_SortingLayerName, layer.m_SortingOrder);
                }

                return tilemap;
            }

            return parentTilemap;
        }

        private TilemapRenderer AddTilemapRendererComponent(GameObject go)
        {
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortOrder = MapRenderConverter.Tiled2Unity(m_MapComponent.m_RenderOrder);

#if UNITY_2018_3_OR_NEWER
            if (m_ImportSorting == ImportSorting.CustomSortAxis)
            {
                renderer.mode = TilemapRenderer.Mode.Individual;
            }
            else
            {
                renderer.mode = TilemapRenderer.Mode.Chunk;
            }
#endif
            return renderer;
        }

        private void ProcessLayerDataChunk(GameObject goTilemap, Chunk chunk)
        {
            // Instantiate object to build all colliders for this chunk
            m_CurrentCollisionBuilder = new CollisionBuilder(goTilemap, m_TilePolygonDatabase, SuperImportContext);

            var tileIds = ReadTileIdsFromChunk(chunk);
            PlaceTiles(goTilemap, chunk, tileIds);

            m_CurrentCollisionBuilder.Build(this);
            m_CurrentCollisionBuilder = null;
        }

        private List<uint> ReadTileIdsFromChunk(Chunk chunk)
        {
            List<uint> tileIds = new List<uint>(chunk.Width * chunk.Height);

            if (chunk.Encoding == DataEncoding.Xml)
            {
                ReadTileIds_Xml(chunk.XmlChunk, ref tileIds);
            }
            else if (chunk.Encoding == DataEncoding.Csv)
            {
                ReadTileIds_Csv(chunk.XmlChunk, ref tileIds);
            }
            else if (chunk.Encoding == DataEncoding.Base64)
            {
                ReadTileIds_Base64(chunk.XmlChunk, chunk.Compression, ref tileIds);
            }
            else
            {
                ReportError("Unhandled encoding type ({0}) used for map layer data.", chunk.Encoding);
            }

            return tileIds;
        }

        private void PlaceTiles(GameObject goTilemap, Chunk chunk, List<uint> tileIds)
        {
            // Only report each missing tile Id once
            HashSet<int> badTiles = new HashSet<int>();

            for (int i = 0; i < tileIds.Count; i++)
            {
                uint utId = tileIds[i];
                if (utId != 0)
                {
                    var tileId = new TileIdMath(utId);

                    var cx = i % chunk.Width;
                    var cy = i / chunk.Width;

                    cx += chunk.X;
                    cy += chunk.Y;

                    Vector3Int int3 = m_MapComponent.TiledIndexToGridCell(i, chunk.Width);

                    SuperTile tile;
                    if (m_GlobalTileDatabase.TryGetTile(tileId.JustTileId, out tile))
                    {
                        PlaceTile(goTilemap, cx, cy, tile, int3, tileId);
                    }
                    else if (!badTiles.Contains(tileId.JustTileId))
                    {
                        ReportError("Could not find tile {0}. Make sure the tilesets were successfully imported.", tileId.JustTileId);
                        badTiles.Add(tileId.JustTileId);
                    }
                }
            }
        }

        private void PlaceTile(GameObject goTilemap, int cx, int cy, SuperTile tile, Vector3Int pos3, TileIdMath tileId)
        {
            if (m_TilesAsObjects)
            {
                PlaceTileAsObject(goTilemap, tile, cx, cy, tileId, pos3);
            }
            else
            {
                PlaceTileAsTile(goTilemap, tile, tileId, pos3);
            }
        }

        private void PlaceTileAsObject(GameObject goTilemap, SuperTile tile, int cx, int cy, TileIdMath tileId, Vector3Int pos3)
        {
            Assert.IsNotNull(goTilemap.GetComponentInParent<SuperMap>());
            Assert.IsNotNull(goTilemap.GetComponentInParent<SuperLayer>());

            var superMap = goTilemap.GetComponentInParent<SuperMap>();
            var superLayer = goTilemap.GetComponentInParent<SuperLayer>();
            var color = new Color(1, 1, 1, superLayer.CalculateOpacity());

            string tileName = string.Format("tile ({0}, {1})", cx, cy);
            var goTRS = new GameObject(string.Format("{0} (TRS)", tileName));
            goTilemap.AddChildWithUniqueName(goTRS);

            // Create a faux SuperObject component to add to our placed tile
            // We need this in case we have custom scripts that are looking for tile object via component
            {
                var tileObject = goTRS.AddComponent<SuperObject>();
                tileObject.m_Id = m_ObjectIdCounter++;
                tileObject.m_TiledName = string.Format("AsObject_{0}", tileObject.m_Id);
                tileObject.m_X = pos3.x * superMap.m_TileWidth;
                tileObject.m_Y = -pos3.y * superMap.m_TileHeight;
                tileObject.m_Width = tile.m_Width;
                tileObject.m_Height = tile.m_Height;
                tileObject.m_TileId = (uint)tile.m_TileId;
                tileObject.m_Visible = true;

                // Does the tile have any properties?
                if (!tile.m_CustomProperties.IsEmpty())
                {
                    var component = tileObject.gameObject.AddComponent<SuperCustomProperties>();
                    component.m_Properties = new List<CustomProperty>();
                    component.m_Properties.CombineFromSource(tile.m_CustomProperties);
                }
            }

            Vector3 translate, rotate, scale;
            tile.GetTRS(tileId.FlipFlags, out translate, out rotate, out scale);

            var cellPos = superMap.MapCoordinatesToPositionPPU(pos3.x, pos3.y);
            translate.x += cellPos.x;
            translate.y -= cellPos.y;

            // If this is an isometric map than we have an additional translate to consider to place the tile
            if (m_MapComponent.m_Orientation == MapOrientation.Isometric)
            {
                translate.y -= m_MapComponent.m_TileHeight * 0.5f * SuperImportContext.Settings.InversePPU;
            }

            // Add the game object for the tile
            goTRS.transform.localPosition = translate;
            goTRS.transform.localRotation = Quaternion.Euler(rotate);
            goTRS.transform.localScale = scale;

            // Add the sprite renderer component
            var renderer = goTRS.AddComponent<SpriteRenderer>();
            renderer.sprite = tile.m_Sprite;
            renderer.color = color;
            AssignMaterial(renderer);
            AssignSortingLayer(renderer, superLayer.m_SortingLayerName, superLayer.m_SortingOrder);

            if (!tile.m_AnimationSprites.IsEmpty())
            {
                var tileAnimator = goTRS.AddComponent<TileObjectAnimator>();
                tileAnimator.m_AnimationFramerate = SuperImportContext.Settings.AnimationFramerate;
                tileAnimator.m_AnimationSprites = tile.m_AnimationSprites;
            }

            // Add any colliders that may be on the tile object
            tile.AddCollidersForTileObject(goTRS, SuperImportContext);
        }

        private void PlaceTileAsTile(GameObject goTilemap, SuperTile tile, TileIdMath tileId, Vector3Int pos3)
        {
            // Burn our layer index into the z component of the tile position
            // This is needed for when using a custom sort axis
            pos3.z = m_TileLayerCounter;

            // Set the flip data
            var tilemapData = goTilemap.GetComponentInParent<TilemapData>();
            tilemapData.SetFlipFlags(pos3, tileId.FlipFlags);

            // Set the tile
            var tilemap = goTilemap.GetComponentInParent<Tilemap>();
            tilemap.SetTile(pos3, tile);

            // Do we have any colliders on the tile to be gathered?
            m_CurrentCollisionBuilder.PlaceTileColliders(m_MapComponent, tile, tileId, pos3);
        }

        private void ReadTileIds_Xml(XElement xElement, ref List<uint> tileIds)
        {
            foreach (var xTile in xElement.Elements("tile"))
            {
                tileIds.Add(xTile.GetAttributeAs<uint>("gid", 0));
            }
        }

        private void ReadTileIds_Csv(XElement xElement, ref List<uint> tileIds)
        {
            // Splitting line-by-line reducues out-of-memory exceptions for really large maps
            // (Really large maps should be avoided, however)
            string data = xElement.Value;
            StringReader reader = new StringReader(data);
            string line = string.Empty;
            do
            {
                line = reader.ReadLine();
                if (!String.IsNullOrEmpty(line))
                {
                    var datum = from val in line.Split(',')
                                where !String.IsNullOrEmpty(val)
                                select Convert.ToUInt32(val);

                    tileIds.AddRange(datum);
                }

            } while (line != null);
        }

        private void ReadTileIds_Base64(XElement xElement, DataCompression compression, ref List<uint> tileIds)
        {
            string data = xElement.Value;

            if (compression == DataCompression.None)
            {
                tileIds = data.Base64ToBytes().ToUInts().ToList();
            }
            else if (compression == DataCompression.Gzip)
            {
                try
                {
                    tileIds = data.Base64ToBytes().GzipDecompress().ToUInts().ToList();
                }
                catch
                {
                    tileIds.Clear();
                    ReportError("Gzip compression is not supported on your development platform. Change Tile Layer Format to another type in Tiled.");
                }
            }
            else if (compression == DataCompression.Zlib)
            {
                try
                {
                    tileIds = data.Base64ToBytes().ZlibDeflate().ToUInts().ToList();
                }
                catch
                {
                    tileIds.Clear();
                    ReportError("zlib compression is not supported on your development platform. Change Tile Layer Format to another type in Tiled.");
                }
            }
            else
            {
                ReportError("Unhandled compression type ({0}) used for map layer data", compression);
            }
        }
    }
}
