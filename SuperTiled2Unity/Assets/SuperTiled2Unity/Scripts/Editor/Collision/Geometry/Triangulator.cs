using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperTiled2Unity.Editor.LibTessDotNet;
using UnityEngine;

namespace SuperTiled2Unity.Editor.Geometry
{
    // Input is a ClipperLib solution and output is a collection of triangles
    public class Triangulator
    {
        public List<Vector2[]> TriangulateClipperSolution(ClipperLib.PolyTree solution)
        {
            var tess = new Tess();
            tess.NoEmptyPolygons = true;

            // Add a contour for each part of the solution tree
            ClipperLib.PolyNode node = solution.GetFirst();
            while (node != null)
            {
                // Only interested in closed paths
                if (!node.IsOpen)
                {
                    // Add a new countor. Holes are automatically generated.
                    var vertices = node.Contour.Select(pt => new ContourVertex { Position = new Vec3 { X = pt.X, Y = pt.Y, Z = 0 } }).ToArray();
                    tess.AddContour(vertices);
                }
                node = node.GetNext();
            }

            return TrianglesFromTessellator(tess);
        }

        public List<Vector2[]> TriangulatePolygon(Vector2[] points)
        {
            var tess = new Tess();
            tess.NoEmptyPolygons = true;

            var vertices = points.Select(pt => new ContourVertex { Position = new Vec3 { X = pt.x, Y = pt.y, Z = 0 } }).ToArray();
            tess.AddContour(vertices);

            return TrianglesFromTessellator(tess);
        }

        private List<Vector2[]> TrianglesFromTessellator(Tess tess)
        {
            var triangles = new List<Vector2[]>();

            // Do the tessellation
            tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

            // Extract the triangles
            int numTriangles = tess.ElementCount;
            for (int i = 0; i < numTriangles; i++)
            {
                var v0 = tess.Vertices[tess.Elements[i * 3 + 0]].Position;
                var v1 = tess.Vertices[tess.Elements[i * 3 + 1]].Position;
                var v2 = tess.Vertices[tess.Elements[i * 3 + 2]].Position;

                var triangle = new List<Vector2>()
                {
                    new Vector2(v0.X, v0.Y),
                    new Vector2(v1.X, v1.Y),
                    new Vector2(v2.X, v2.Y),
                };

                // Assume each triangle needs to be CCW
                float cross = GeoMath.Cross(triangle[0], triangle[1], triangle[2]);
                if (cross > 0)
                {
                    triangle.Reverse();
                }

                triangles.Add(triangle.ToArray());
            }

            return triangles;
        }

    }
}
