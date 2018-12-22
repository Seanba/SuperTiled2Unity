using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            bool tilesAsObjects = m_MapComponent.m_Orientation == MapOrientation.Isometric || m_TilesAsObjects;

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

                    // Possition the chunk
                    Vector3Int int3 = m_MapComponent.TilePositionToGridPosition(chunk.X, chunk.Y);
                    Vector3 translate = SuperImportContext.MakePoint(int3.x, int3.y);
                    translate.x *= m_MapComponent.m_TileWidth;
                    translate.y *= m_MapComponent.m_TileHeight;

                    goChunk.transform.localPosition = translate;

                    // Create the tilemap for the layer if needed
                    if (!tilesAsObjects)
                    {
                        var tilemap = goChunk.AddComponent<Tilemap>();
                        tilemap.tileAnchor = Vector3.zero;
                        tilemap.animationFrameRate = SuperImportContext.Settings.AnimationFramerate;
                        tilemap.color = new Color(1, 1, 1, superComp.CalculateOpacity());

                        // Create the renderer for the layer
                        var renderer = goChunk.AddComponent<TilemapRenderer>();
                        renderer.sortOrder = MapRenderConverter.Tiled2Unity(m_MapComponent.m_RenderOrder);
                        AssignMaterial(renderer);
                        AssignSortingLayer(renderer, superComp.m_SortingLayerName, superComp.m_SortingOrder);
                    }

                    ProcessLayerDataChunk(goChunk, chunk);
                }
            }
            else
            {
                // Regular maps only have one chunk with the Tilemap and TileRenderer being on the layer object

                // Add the tilemap components if needed
                if (!tilesAsObjects)
                {
                    var tilemap = goLayer.AddComponent<Tilemap>();
                    tilemap.tileAnchor = Vector3.zero;
                    tilemap.animationFrameRate = SuperImportContext.Settings.AnimationFramerate;
                    tilemap.color = new Color(1, 1, 1, superComp.CalculateOpacity());

                    // Create the renderer for the layer
                    var renderer = goLayer.AddComponent<TilemapRenderer>();
                    renderer.sortOrder = MapRenderConverter.Tiled2Unity(m_MapComponent.m_RenderOrder);
                    AssignMaterial(renderer);
                    AssignSortingLayer(renderer, superComp.m_SortingLayerName, superComp.m_SortingOrder);
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

        private void ProcessLayerDataChunk(GameObject goTilemap, Chunk chunk)
        {
            // Instantiate object to build all colliders for this chunk
            m_CurrentCollisionBuilder = new CollisionBuilder(goTilemap, m_TilePolygonDatabase, SuperImportContext);

            var tileIds = ReadTileIdsFromChunk(chunk);
            PlaceTiles(goTilemap, chunk, tileIds);

            m_CurrentCollisionBuilder.Build();
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

                    // Y position is tricky. We want the origin to be the top-left corner
                    Vector3Int int3 = m_MapComponent.TileIndexToGridPosition(i, chunk.Width);
                    int3.y = -int3.y;
                    int3.y -= 1;

                    SuperTile tile;
                    if (m_GlobalTileDatabase.TryGetTile(tileId.JustTileId, out tile))
                    {
                        PlaceTile(goTilemap, cx, cy, tile, int3, tileId);
                    }
                    else if (!badTiles.Contains(tileId.JustTileId))
                    {
                        ReportError("Could not find tile {0}. Your imported map will have holes until this is fixed.", tileId.JustTileId);
                        badTiles.Add(tileId.JustTileId);
                    }
                }
            }
        }

        private void PlaceTile(GameObject goTilemap, int cx, int cy, SuperTile tile, Vector3Int pos3, TileIdMath tileId)
        {
            // We're either placing tiles or objects as tiles
            Assert.IsTrue(m_TilesAsObjects || m_MapComponent.m_Orientation == MapOrientation.Isometric || goTilemap.GetComponent<Tilemap>() != null);

            // Bake transform flags into the z component (for flipped/rotated tiles)
            pos3.z = tileId.PlacementZ;

            bool tilesAsObjects = m_MapComponent.m_Orientation == MapOrientation.Isometric || m_TilesAsObjects;
            if (tilesAsObjects)
            {
                PlaceTileAsObject(goTilemap, tile, cx, cy, pos3);
            }
            else
            {
                PlaceTileAsTile(goTilemap, tile, tileId, pos3);
            }
        }

        private void PlaceTileAsObject(GameObject goTilemap, SuperTile tile, int cx, int cy, Vector3Int pos3)
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
            tile.GetTRS((FlipFlags)pos3.z, out translate, out rotate, out scale);

            translate.x += pos3.x * superMap.CellSize.x;
            translate.y += pos3.y * superMap.CellSize.y;

            // If this is an isometric map than we have an additional translate to consider to place the tile
            if (m_MapComponent.m_Orientation == MapOrientation.Isometric)
            {
                translate.x -= m_MapComponent.m_TileWidth * 0.5f * SuperImportContext.Settings.InversePPU;
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
            var tilemap = goTilemap.GetComponent<Tilemap>();
            tilemap.SetTile(pos3, tile);

            // Do we have any colliders on the tile to be gathered?
            m_CurrentCollisionBuilder.PlaceTileColliders(tile, tileId, pos3);
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
                tileIds = data.Base64ToBytes().GzipDecompress().ToUInts().ToList();
            }
            else if (compression == DataCompression.Zlib)
            {
                tileIds = data.Base64ToBytes().ZlibDeflate().ToUInts().ToList();
            }
            else
            {
                ReportError("Unhandled compression type ({0}) used for map layer data", compression);
            }
        }
    }
}
