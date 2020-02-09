using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public static class SuperTileExtensions
    {
        public static void AddCollidersForTileObject(this SuperTile tile, GameObject goParent, SuperImportContext importContext)
        {
            Assert.IsNotNull(tile);
            Assert.IsNotNull(goParent);
            Assert.IsNotNull(importContext);

            if (!tile.m_CollisionObjects.IsEmpty())
            {
                foreach (var collision in tile.m_CollisionObjects)
                {
                    collision.AddCollider(tile, goParent, importContext);
                }
            }
        }
    }
}
