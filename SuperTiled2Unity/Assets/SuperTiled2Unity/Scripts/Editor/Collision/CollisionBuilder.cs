using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using SuperTiled2Unity;

namespace SuperTiled2Unity.Editor
{
    // Tile Layer importer uses CollisionBuilder to combine all like-minded collision objects together
    public class CollisionBuilder
    {
        private GameObject m_TilemapGameObject;
        private Tilemap m_Tilemap;
        private Dictionary<uint, TilePolygonCollection> m_TilePolygonDatabase;
        private Dictionary<CollisionClipperKey, CollisionClipper> m_CollisionClippers = new Dictionary<CollisionClipperKey, CollisionClipper>();
        private SuperImportContext m_ImportContext;

        public CollisionBuilder(GameObject goTilemap, Dictionary<uint, TilePolygonCollection> tilePolygonDatabase, SuperImportContext importContext)
        {
            m_TilemapGameObject = goTilemap;
            m_Tilemap = m_TilemapGameObject.GetComponent<Tilemap>();
            m_TilePolygonDatabase = tilePolygonDatabase;
            m_ImportContext = importContext;
        }

        public void PlaceTileColliders(SuperTile tile, TileIdMath tileId, Vector3Int pos)
        {
            Assert.IsNotNull(m_Tilemap, "Need a Tilemap component if we are going to gather tile colliders");

            // Tile y position is always off by one
            pos.y++;

            // Do we have any collider objects defined for this tile?
            if (!tile.m_CollisionObjects.IsEmpty())
            {
                var polygons = AcquireTilePolygonCollection(tile, tileId);

                foreach (var poly in polygons.Polygons)
                {
                    // Offset the polygon so that it is in the location of the tile
                    var tileHeight = m_ImportContext.MakeScalar(tile.m_Height);
                    var tileDiff = m_Tilemap.cellSize.y - tileHeight;
                    var offset = new Vector2(pos.x * m_Tilemap.cellSize.x, pos.y * m_Tilemap.cellSize.y - tileDiff);
                    var points = poly.Points.Select(pt => pt + offset).ToArray();


                    CollisionClipperKey key = poly.MakeKey();
                    CollisionClipper clipper;
                    if (!m_CollisionClippers.TryGetValue(key, out clipper))
                    {
                        // Add a new clipper for the layer
                        clipper = new CollisionClipper();
                        m_CollisionClippers.Add(key, clipper);
                    }

                    // Add the path to our clipper
                    if (poly.IsClosed)
                    {
                        clipper.AddClosedPath(points);
                    }
                    else
                    {
                        clipper.AddOpenPath(points);
                    }
                }
            }
        }

        public void Build()
        {
            // Excute our clippers and add game objects with their solution polygons
            foreach (var pair in m_CollisionClippers)
            {
                var key = pair.Key;
                var clipper = pair.Value;

                clipper.Execute();

                if (clipper.ClosedPaths.Any() || clipper.OpenPaths.Any())
                {
                    var layerId = key.LayerId;
                    
                    if (layerId == 0)
                    {
                        // In this context, default means inherit from tilemap layer
                        layerId = m_TilemapGameObject.layer;
                    }

                    var layerName = LayerMask.LayerToName(layerId);
                    GameObject goCollider = new GameObject("Collision_" + layerName);
                    goCollider.layer = layerId;
                    m_TilemapGameObject.AddChildWithUniqueName(goCollider);

                    // Rigid body is needed for composite collider
                    var rigid = goCollider.AddComponent<Rigidbody2D>();
                    rigid.bodyType = RigidbodyType2D.Static;
                    rigid.simulated = true;

                    // Colliders will be grouped by the composite
                    // This way we have convex polygon paths (in the children) if needed
                    // And we can have complex polygons represented by one object
                    var composite = goCollider.AddComponent<CompositeCollider2D>();
                    composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
                    composite.isTrigger = key.IsTrigger;
                    composite.generationType = CompositeCollider2D.GenerationType.Manual;

                    // Add polygon colliders
                    foreach (var path in clipper.ClosedPaths)
                    {
                        var goPolygon = new GameObject("Polygon");
                        goPolygon.layer = layerId;
                        goCollider.AddChildWithUniqueName(goPolygon);

                        var polyCollider = goPolygon.AddComponent<PolygonCollider2D>();
                        polyCollider.usedByComposite = true;
                        polyCollider.SetPath(0, path);
                        polyCollider.gameObject.AddComponent<SuperColliderComponent>();
                    }

                    // Add Edge colliders
                    foreach (var path in clipper.OpenPaths)
                    {
                        var goPolyline = new GameObject("Polyline");
                        goPolyline.layer = layerId;
                        goCollider.AddChildWithUniqueName(goPolyline);

                        var edgeCollider = goPolyline.AddComponent<EdgeCollider2D>();
                        edgeCollider.points = path;
                        edgeCollider.gameObject.AddComponent<SuperColliderComponent>();
                    }

                    composite.GenerateGeometry();
                }
            }
        }

        private TilePolygonCollection AcquireTilePolygonCollection(SuperTile tile, TileIdMath tileId)
        {
            TilePolygonCollection polygons;
            if (m_TilePolygonDatabase.TryGetValue(tileId.ImportedlTileId, out polygons))
            {
                return polygons;
            }

            // If we're here then we don't have a polygon collection for this tile yet
            polygons = new TilePolygonCollection(tile, tileId, m_ImportContext);
            m_TilePolygonDatabase.Add(tileId.ImportedlTileId, polygons);
            return polygons;
        }
    }
}
