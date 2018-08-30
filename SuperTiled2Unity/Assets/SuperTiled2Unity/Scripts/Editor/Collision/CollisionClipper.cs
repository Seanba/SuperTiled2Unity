using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperTiled2Unity.Editor.ClipperLib;
using SuperTiled2Unity.Editor.Geometry;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class CollisionClipper
    {
        private const float Multiplier = 1024.0f;
        private const float InvMultiplier = 1.0f / Multiplier;

        private Clipper m_Clipper = new Clipper();
        private List<Vector2[]> m_RawOpenPaths = new List<Vector2[]>();

        // Once the clipper is executed we have our list of processed closed and open paths
        public List<Vector2[]> ClosedPaths { get; private set; }
        public List<Vector2[]> OpenPaths { get; private set; }

        public void AddClosedPath(Vector2[] points)
        {
            var path = ToClipperSpace(points);
            m_Clipper.AddPath(path, PolyType.ptSubject, true);
        }

        public void AddOpenPath(Vector2[] points)
        {
            // Open paths do not get clipped but we gather them so they can be combined when we execute
            m_RawOpenPaths.Add(points);
        }

        public void Execute()
        {
            PolyTree solution = new PolyTree();
            m_Clipper.Execute(ClipType.ctUnion, solution, PolyFillType.pftNonZero, PolyFillType.pftEvenOdd);

            AddConvexPolygonsFromSolution(solution);
            CombineLines();
        }

        private void AddConvexPolygonsFromSolution(PolyTree solution)
        {
            ClosedPaths = new List<Vector2[]>();

            // Triangulate the solution
            var triangulator = new Triangulator();
            var triangles = triangulator.TriangulateClipperSolution(solution);

            // Gather triangles into a collection of convex polygons
            var composition = new ComposeConvexPolygons();
            var convexPolygons = composition.Compose(triangles);

            foreach (var poly in convexPolygons)
            {
                var transformed = FromClipperSpace(poly);
                ClosedPaths.Add(transformed);
            }
        }

        private void CombineLines()
        {
            OpenPaths = new List<Vector2[]>();

            var reduction = new PolylineReduction();

            foreach (var line in m_RawOpenPaths)
            {
                var transformed = ToClipperSpace(line);
                reduction.AddLine(transformed);
            }

            var polyLines = reduction.Reduce();

            foreach (var lines in polyLines)
            {
                var transformed = FromClipperSpace(lines);
                OpenPaths.Add(transformed);
            }
        }

        private List<IntPoint> ToClipperSpace(Vector2[] points)
        {
            return points.Select(pt => new IntPoint(pt.x * Multiplier, pt.y * Multiplier)).ToList();
        }

        private Vector2[] FromClipperSpace(List<IntPoint> points)
        {
            return points.Select(pt => new Vector2(pt.X * InvMultiplier, pt.Y * InvMultiplier)).ToArray();
        }

        private Vector2[] FromClipperSpace(IEnumerable<Vector2> points)
        {
            return points.Select(pt => new Vector2(pt.x * InvMultiplier, pt.y * InvMultiplier)).ToArray();
        }
    }
}
