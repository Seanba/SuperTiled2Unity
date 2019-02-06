using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperTiled2Unity.Dictionaries;

namespace SuperTiled2Unity
{
    public class TilemapData : MonoBehaviour
    {
        [SerializeField]
        private Dictionary_Vector3Int_FlipFlags m_TilePositionFlipFlags = new Dictionary_Vector3Int_FlipFlags();

        public void SetFlipFlags(Vector3Int pos3, FlipFlags flags)
        {
            if (flags != 0)
            {
                m_TilePositionFlipFlags.Add(pos3, flags);
            }
        }

        public FlipFlags GetFlipFlags(Vector3Int pos3)
        {
            FlipFlags flags;
            if (m_TilePositionFlipFlags.TryGetValue(pos3, out flags))
            {
                return flags;
            }

            return 0;
        }
    }
}
