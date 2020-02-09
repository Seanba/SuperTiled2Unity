using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor.Geometry
{
    // Keeps a collection of polygon edges that are shared bewteen two polygons
    // Assumes that all polygons have CCW winding to them
    public class PolygonEdgeGroup
    {
        public List<PolygonEdge> PolygonEdges { get; set; }

        public void Initialize(List<Vector2[]> polygons)
        {
            this.PolygonEdges = new List<PolygonEdge>();

            int polygonId = 0;
            foreach (var polygon in polygons)
            {
                // Our polygon will be added to each edge
                CompositionPolygon compPolygon = new CompositionPolygon(polygon, polygonId++);

                // Process all edges of the polygon
                for (int p = polygon.Length - 1, q = 0; q < polygon.Length; p = q++)
                {
                    Vector2 P = polygon[p];
                    Vector2 Q = polygon[q];

                    // The clockwise edge may already exist if it was added by an earlier polygon as the counter-clockwise edge
                    // If so, add this polygon as the CW partner of that edge
                    PolygonEdge edge = this.PolygonEdges.FirstOrDefault(e => e.P == Q && e.Q == P);
                    if (edge != null)
                    {
                        // Add ourselves as the Minor/CW partner
                        edge.AssignMinorPartner(compPolygon);
                        compPolygon.AddEdge(edge);
                    }
                    else
                    {
                        // If this edge is new to the collection then add it with this polygon being the CCW partner
                        PolygonEdge newEdge = new PolygonEdge(compPolygon, p);
                        compPolygon.AddEdge(newEdge);
                        this.PolygonEdges.Add(newEdge);
                    }
                }
            }
        }
    }
}
