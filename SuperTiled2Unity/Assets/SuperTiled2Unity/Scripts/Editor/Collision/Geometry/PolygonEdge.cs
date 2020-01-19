using UnityEngine;

namespace SuperTiled2Unity.Editor.Geometry
{
    // A polygon edge that may be shared with another polygon
    public class PolygonEdge
    {
        public bool HasBeenMerged { get; private set; }

        public Vector2 P { get; private set; }
        public Vector2 Q { get; private set; }
        public float Length2 { get; private set; }

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

            this.HasBeenMerged = false;

            // P and Q make up our edge
            int q = (p + 1) % compPolygon.Points.Count;
            this.P = compPolygon.Points[p];
            this.Q = compPolygon.Points[q];

            // Create a compositional polygon with our edge
            this.MajorPartner = compPolygon;
            this.MajorPartner_pIndex = p;
            this.MajorPartner_qIndex = q;

            // Calculate the squared length
            float x = (this.P.x - this.Q.x);
            float y = (this.P.y - this.Q.y);
            this.Length2 = (x * x) + (y * y);
        }

        public void AssignMinorPartner(CompositionPolygon polygon)
        {
            Debug.Assert(this.MinorPartner == null);
            Debug.Assert(this.MajorPartner != null);

            ReplaceMinor(polygon);
        }

        public void ReplaceMajor(CompositionPolygon polygon)
        {
            this.MajorPartner = polygon;
            this.MajorPartner_pIndex = this.MajorPartner.Points.IndexOf(this.P);
            this.MajorPartner_qIndex = this.MajorPartner.Points.IndexOf(this.Q);
        }

        public void ReplaceMinor(CompositionPolygon polygon)
        {
            this.MinorPartner = polygon;
            this.MinorPartner_pIndex = this.MinorPartner.Points.IndexOf(this.P);
            this.MinorPartner_qIndex = this.MinorPartner.Points.IndexOf(this.Q);
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
                var A = this.MajorPartner.PrevPoint(this.MajorPartner_pIndex);
                var B = this.MajorPartner.Points[this.MajorPartner_pIndex];
                var C = this.MinorPartner.NextPoint(this.MinorPartner_pIndex);
                float cross = GeoMath.Cross(A, B, C);
                if (cross  > 0)
                    return false;
            }

            // Can merge point Q of Major/CCW partner?
            {
                // A = CWW[Q + 1]
                // B = CWW[Q]
                // C = CW[Q-1]
                var A = this.MajorPartner.NextPoint(this.MajorPartner_qIndex);
                var B = this.MajorPartner.Points[this.MajorPartner_qIndex];
                var C = this.MinorPartner.PrevPoint(this.MinorPartner_qIndex);
                float cross = GeoMath.Cross(A, B, C);
                if (cross < 0)
                    return false;
            }

            return true;
        }

        public void MergePolygons()
        {
            Debug.Assert(this.HasBeenMerged == false);

            // The major polygon will absorb the minor
            this.MajorPartner.AbsorbPolygon(this.MajorPartner_qIndex, this.MinorPartner, this.MinorPartner_pIndex);

            // All edges that referened the minor will need to reference the major
            this.MinorPartner.ReplaceEdgesWithPolygon(this.MajorPartner, this);

            // All edges that reference the major will need their P/Q indices updated
            this.MajorPartner.UpdateEdgeIndices(this);

            // This edge has now been processed and we shouldn't merge on it again
            this.HasBeenMerged = true;
        }

        public void UpdateIndices(CompositionPolygon polygon)
        {
            if (polygon == this.MajorPartner)
            {
                this.MajorPartner_pIndex = polygon.Points.IndexOf(this.P);
                this.MajorPartner_qIndex = polygon.Points.IndexOf(this.Q);
            }
            else if (polygon == MinorPartner)
            {
                this.MinorPartner_pIndex = polygon.Points.IndexOf(this.P);
                this.MinorPartner_qIndex = polygon.Points.IndexOf(this.Q);
            }
        }
    }
}
