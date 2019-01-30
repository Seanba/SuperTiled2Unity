using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class TilePolygon
    {
        public int ColliderLayerId { get; set; }
        public string ColliderLayerName { get; set; }
        public bool IsTrigger { get; set; }
        public Vector2[] Points { get; set; }
        public bool IsClosed { get; set; }

        public CollisionClipperKey MakeKey()
        {
            return new CollisionClipperKey(ColliderLayerId, ColliderLayerName, IsTrigger);
        }
    }
}
