using UnityEngine;

namespace SuperTiled2Unity.Editor.Geometry
{
    // A polygon edge that may be shared with another polygon
    public class PolygonEdge
    {
        public bool HasBeenMerged { get; private set; }

        public Vector2 P { get; }
        public Vector2 Q { get; }
        public float Length2 { get; }

        // Our Major partner (the edge PQ will be counter-clockwise on this polygon)
        // When we merge polygons it is always the Major partner that absorbs
        public CompositionPolygon MajorPartner { get; private set; }
        public int MajorPartner_pIndex { get; private set; }
        public int MajorPartner_qIndex { get; private set; }

        // Our Minor partner (the edge PQ will clockwise on this polygon)
        public CompositionPolygon MinorPartner { get; private set; }
        public int MinorPartner_pIndex { get; private set; }
        public int MinorPartner_qIndex { get; private set; }

        public PolygonEdge(CompositionPolygon compPolygon, int p)
        {
            Debug.Assert(compPolygon.Points.Count >= 3);

            HasBeenMerged = false;

            // P and Q make up our edge
            int q = (p + 1) % compPolygon.Points.Count;
            P = compPolygon.Points[p];
            Q = compPolygon.Points[q];

            // Create a compositional polygon with our edge
            MajorPartner = compPolygon;
            MajorPartner_pIndex = p;
            MajorPartner_qIndex = q;

            // Calculate the squared length
            float x = (P.x - Q.x);
            float y = (P.y - Q.y);
            Length2 = (x * x) + (y * y);
        }

        public void AssignMinorPartner(CompositionPolygon polygon)
        {
            Debug.Assert(MinorPartner == null);
            Debug.Assert(MajorPartner != null);

            ReplaceMinor(polygon);
        }

        public void ReplaceMajor(CompositionPolygon polygon)
        {
            MajorPartner = polygon;
            MajorPartner_pIndex = MajorPartner.Points.IndexOf(P);
            MajorPartner_qIndex = MajorPartner.Points.IndexOf(Q);
        }

        public void ReplaceMinor(CompositionPolygon polygon)
        {
            MinorPartner = polygon;
            MinorPartner_pIndex = MinorPartner.Points.IndexOf(P);
            MinorPartner_qIndex = MinorPartner.Points.IndexOf(Q);
        }

        public bool CanMergePolygons()
        {
            // The two polygon partners can be merged if the two vectors on each point where they would merge don't create a concave polygon
            // Concave testing is done through a cross product and assumes CCW winding of the polyon points

            // Can merge point P of the Major/CCW partner?
            {
                // A = CWW[P - 1]
                // B = CWW[P]
                // C = CW[P + 1]
                var A = MajorPartner.PrevPoint(MajorPartner_pIndex);
                var B = MajorPartner.Points[MajorPartner_pIndex];
                var C = MinorPartner.NextPoint(MinorPartner_pIndex);
                float cross = GeoMath.Cross(A, B, C);
                if (cross  > 0)
                    return false;
            }

            // Can merge point Q of Major/CCW partner?
            {
                // A = CWW[Q + 1]
                // B = CWW[Q]
                // C = CW[Q-1]
                var A = MajorPartner.NextPoint(MajorPartner_qIndex);
                var B = MajorPartner.Points[MajorPartner_qIndex];
                var C = MinorPartner.PrevPoint(MinorPartner_qIndex);
                float cross = GeoMath.Cross(A, B, C);
                if (cross < 0)
                    return false;
            }

            return true;
        }

        public void MergePolygons()
        {
            Debug.Assert(HasBeenMerged == false);

            // The major polygon will absorb the minor
            MajorPartner.AbsorbPolygon(MajorPartner_qIndex, MinorPartner, MinorPartner_pIndex);

            // All edges that referened the minor will need to reference the major
            MinorPartner.ReplaceEdgesWithPolygon(MajorPartner, this);

            // All edges that reference the major will need their P/Q indices updated
            MajorPartner.UpdateEdgeIndices(this);

            // This edge has now been processed and we shouldn't merge on it again
            HasBeenMerged = true;
        }

        public void UpdateIndices(CompositionPolygon polygon)
        {
            if (polygon == this.MajorPartner)
            {
                MajorPartner_pIndex = polygon.Points.IndexOf(P);
                MajorPartner_qIndex = polygon.Points.IndexOf(Q);
            }
            else if (polygon == MinorPartner)
            {
                MinorPartner_pIndex = polygon.Points.IndexOf(P);
                MinorPartner_qIndex = polygon.Points.IndexOf(Q);
            }
        }
    }
}
