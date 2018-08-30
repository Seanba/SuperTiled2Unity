using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperTiled2Unity.Editor
{
    public struct CollisionClipperKey : IEquatable<CollisionClipperKey>
    {
        private readonly int m_LayerId;
        private readonly bool m_IsTrigger;

        public CollisionClipperKey(int layerId, bool isTrigger)
        {
            m_LayerId = layerId;
            m_IsTrigger = isTrigger;
        }

        public int LayerId { get { return m_LayerId; } }

        public bool IsTrigger { get { return m_IsTrigger; } }

        public override int GetHashCode()
        {
            return m_LayerId.GetHashCode() ^ m_IsTrigger.GetHashCode();
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
                other.m_IsTrigger.Equals(m_IsTrigger);
        }
    }
}
