using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SuperTiled2Unity.Editor.ClipperLib;

namespace SuperTiled2Unity.Editor.Geometry
{
    // Join line segments into polylines
    class PolylineReduction
    {
        private static int CurrentPolylineId = 0;

        // Cheap internal class for grouping similar polyines (that differ only in direction) by an assinged Id
        public class InternalPolyline
        {
            public int Id;
            public List<IntPoint> Points = new List<IntPoint>();
        }

        // Hash polylines by their endpoints so we can combine them
        SD.Tools.Algorithmia.GeneralDataStructures.MultiValueDictionary<IntPoint, InternalPolyline> m_TablePolyline = new SD.Tools.Algorithmia.GeneralDataStructures.MultiValueDictionary<ClipperLib.IntPoint, InternalPolyline>();

        public void AddLine(List<ClipperLib.IntPoint> points)
        {
            PolylineReduction.CurrentPolylineId++;

            // Get rid of mid-points along the line that are not needed
            points = RemovePointsOnLine(points);

            // Always add the polyline forward
            InternalPolyline forwards = new InternalPolyline();
            forwards.Id = PolylineReduction.CurrentPolylineId;
            forwards.Points.AddRange(points);

            m_TablePolyline.Add(forwards.Points.Last(), forwards);

            // Add the polyline backwards too if the end-points are different
            // Make sure the Id is the same though
            if (points.First() != points.Last())
            {
                InternalPolyline backwards = new InternalPolyline();
                backwards.Id = PolylineReduction.CurrentPolylineId;
                backwards.Points.AddRange(points);
                backwards.Points.Reverse();

                m_TablePolyline.Add(backwards.Points.Last(), backwards);
            }
        }

        private bool AreNormalsEquivalent(ClipperLib.DoublePoint n0, ClipperLib.DoublePoint n1)
        {
            const double epsilon = 1.0f / 1024.0f;
            double ax = Math.Abs(n0.X - n1.X);
            double ay = Math.Abs(n0.Y - n1.Y);
            return (ax < epsilon) && (ay < epsilon);
        }

        private List<ClipperLib.IntPoint> RemovePointsOnLine(List<ClipperLib.IntPoint> points)
        {
            int index = 0;
            while (index < points.Count - 2)
            {
                ClipperLib.DoublePoint normal0 = ClipperLib.ClipperOffset.GetUnitNormal(points[index], points[index + 1]);
                ClipperLib.DoublePoint normal1 = ClipperLib.ClipperOffset.GetUnitNormal(points[index], points[index + 2]);

                if (AreNormalsEquivalent(normal0, normal1))
                {
                    points.RemoveAt(index + 1);
                }
                else
                {
                    index++;
                }
            }

            return points;
        }

        private void CombinePolyline(InternalPolyline line0, InternalPolyline line1)
        {
            // Assumes Line0 and Line1 have the same end-points
            // We reverse Line1 and remove its first end-point
            List<ClipperLib.IntPoint> combined = new List<ClipperLib.IntPoint>();
            combined.AddRange(line0.Points);

            line1.Points.Reverse();
            line1.Points.RemoveAt(0);
            combined.AddRange(line1.Points);

            AddLine(combined);
        }

        private void RemovePolyline(InternalPolyline polyline)
        {
            var removes = from pairs in m_TablePolyline
                          from line in pairs.Value
                          where line.Id == polyline.Id
                          select line;

            var removeList = removes.ToList();
            foreach (var rem in removeList)
            {
                m_TablePolyline.Remove(rem.Points.Last(), rem);
            }
        }

        // Returns a list of polylines (each polyine is itself a list of points)
        public List<List<ClipperLib.IntPoint>> Reduce()
        {
            // Combine all the polylines together
            // We should end up with a table of polylines where each key has only one entry
            var set = m_TablePolyline.FirstOrDefault(kvp => kvp.Value.Count > 1);
            while (set.Value != null)
            {
                // The set is guaranteed to have at least two polylines in it
                // Combine the first and reverse-second polylines into a bigger polyline
                // Remove both polylines from the table
                // Add the combined polyline
                var polylines = set.Value.ToList();
                InternalPolyline line0 = polylines[0];
                InternalPolyline line1 = polylines[1];

                RemovePolyline(line0);
                RemovePolyline(line1);
                CombinePolyline(line0, line1);

                // Look for the next group of polylines that share an endpoint
                set = m_TablePolyline.FirstOrDefault(kvp => kvp.Value.Count > 1);
            }

            // The resulting lines will be in the table twice so make the list unique on Polyline Id
            var unique = from pairs in m_TablePolyline
                         from line in pairs.Value
                         select line;

            unique = unique.GroupBy(ln => ln.Id).Select(grp => grp.First());

            var lines = from l in unique
                        select l.Points;

            return lines.ToList();
        }

    }
}
