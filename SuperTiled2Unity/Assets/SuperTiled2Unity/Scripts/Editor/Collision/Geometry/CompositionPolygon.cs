using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor.Geometry
{
    // For compositional considerations, a Polygon is a group of points and edges
    // This allows us to merge polygons along edges
    public class CompositionPolygon
    {
        public int InitialId { get; private set; }
        public List<Vector2> Points { get; private set; }
        public List<PolygonEdge> Edges { get; private set; }

        // A polygon starts off as a triangle with one edge
        // Other points and edges are added to the polygon during merge
        public CompositionPolygon(IEnumerable<Vector2> points, int initialId)
        {
            this.InitialId = initialId;
            this.Points = new List<Vector2>();
            this.Edges = new List<PolygonEdge>();

            this.Points.AddRange(points);
        }

        public void AddEdge(PolygonEdge edge)
        {
            this.Edges.Add(edge);
        }

        public int NextIndex(int index)
        {
            Assert.IsTrue(index >= 0);

            return (index + 1) % this.Points.Count;
        }

        public int PrevIndex(int index)
        {
            Assert.IsTrue(index >= 0);

            if (index == 0)
            {
                return this.Points.Count - 1;
            }

            return (index - 1) % this.Points.Count;
        }

        public Vector2 NextPoint(int index)
        {
            index = NextIndex(index);
            return this.Points[index];
        }

        public Vector2 PrevPoint(int index)
        {
            index = PrevIndex(index);
            return this.Points[index];
        }

        public void AbsorbPolygon(int q, CompositionPolygon minor, int pMinor)
        {
            // Insert Minor points Minor[P+1] ... Minor[Q-1] into Major, inserted at Major[Q]
            // Same as inserting numPoints-2 starting at index qMinor+1
            int numMinorPoints = minor.Points.Count - 2;

            List<Vector2> pointsToInsert = new List<Vector2>();
            for (int i = 0; i < numMinorPoints; ++i)
            {
                int qInsert = (pMinor + 1 + i) % minor.Points.Count;
                pointsToInsert.Add(minor.Points[qInsert]);
            }

            this.Points.InsertRange(q, pointsToInsert);

            // Absorb the edges from our minor polygon too so that future absoptions carry through
            foreach (var minorEdge in minor.Edges)
            {
                if (!this.Edges.Contains(minorEdge))
                {
                    this.Edges.Add(minorEdge);
                }
            }
        }

        public void ReplaceEdgesWithPolygon(CompositionPolygon replacement, PolygonEdge ignoreEdge)
        {
            // This polygon is going away as it was merged with another
            // All edges this polygon referenced will need to reference the replacement instead
            foreach (var edge in this.Edges)
            {
                if (edge == ignoreEdge)
                    continue;

                Assert.IsFalse(edge.MajorPartner == this && edge.MinorPartner == this);

                if (edge.MajorPartner == this)
                {
                    edge.ReplaceMajor(replacement);
                }
                else if (edge.MinorPartner == this)
                {
                    edge.ReplaceMinor(replacement);
                }
            }
        }

        public void UpdateEdgeIndices(PolygonEdge ignoreEdge)
        {
            // All of our edges need to update their indices to us
            foreach (var edge in this.Edges)
            {
                if (edge == ignoreEdge)
                    continue;

                edge.UpdateIndices(this);
            }
        }
    }
}
