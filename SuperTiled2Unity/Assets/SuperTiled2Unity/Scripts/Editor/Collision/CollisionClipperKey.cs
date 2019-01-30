using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperTiled2Unity.Editor
{
    public struct CollisionClipperKey : IEquatable<CollisionClipperKey>
    {
        private readonly int m_LayerId;
        private readonly string m_LayerName;
        private readonly bool m_IsTrigger;

        public CollisionClipperKey(int layerId, string layerName, bool isTrigger)
        {
            m_LayerId = layerId;
            m_LayerName = layerName;
            m_IsTrigger = isTrigger;
        }

        public int LayerId { get { return m_LayerId; } }

        public string LayerName { get { return m_LayerName; } }

        public bool IsTrigger { get { return m_IsTrigger; } }

        public override int GetHashCode()
        {
            var result = m_LayerId.GetHashCode();
            result = (result * 397) ^ m_LayerName.GetHashCode();
            result = (result * 397) ^ m_IsTrigger.GetHashCode();
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals((CollisionClipperKey)obj);
        }

        public bool Equals(CollisionClipperKey other)
        {
            return other.m_LayerId.Equals(m_LayerId) &&
                other.m_LayerName.Equals(m_LayerName, StringComparison.OrdinalIgnoreCase) &&
                other.m_IsTrigger.Equals(m_IsTrigger);
        }
    }
}
