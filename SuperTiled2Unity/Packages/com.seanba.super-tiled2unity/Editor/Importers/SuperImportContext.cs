using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_2020_2_OR_NEWER
using AssetImportContext = UnityEditor.AssetImporters.AssetImportContext;
#else
using AssetImportContext = UnityEditor.Experimental.AssetImporters.AssetImportContext;
#endif

namespace SuperTiled2Unity.Editor
{
    public class SuperImportContext
    {
        private static Vector2 NegateY = new Vector2(1, -1);

        private AssetImportContext m_Context;
        private bool? m_IsTriggerOverride;

        public SuperImportContext(AssetImportContext context, ST2USettings settings)
        {
            m_Context = context;
            Settings = settings;
        }

        public ST2USettings Settings { get; private set; }

        public LayerIgnoreMode LayerIgnoreMode { get; private set; }

        public Vector3 TilemapOffset { get; set; }

        public void AddObjectToAsset(string identifier, UnityEngine.Object obj)
        {
            m_Context.AddObjectToAsset(identifier, obj);
        }

        public void AddObjectToAsset(string identifier, UnityEngine.Object obj, Texture2D thumbnail)
        {
            m_Context.AddObjectToAsset(identifier, obj, thumbnail);
        }

        public void SetMainObject(UnityEngine.Object obj)
        {
            m_Context.SetMainObject(obj);
        }

        public int GetNumberOfObjects()
        {
            var objects = new List<UnityEngine.Object>();
#if UNITY_2018_3_OR_NEWER
            m_Context.GetObjects(objects);
#endif
            return objects.Count;
        }

        // Math/space transform functions
        // Points in Tiled have (0, 0) at top left corner of map (+y goes down)
        // Our Unity projects have +y going up and points are transformed by a Pixels Per Unity constant
        public float MakeScalar(float s)
        {
            return s * Settings.InversePPU;
        }

        // Does not negate y
        public Vector2 MakeSize(float x, float y)
        {
            return MakeSize(new Vector2(x, y));
        }

        public Vector2 MakeSize(Vector2 size)
        {
            return size * Settings.InversePPU;
        }

        public Vector2 MakePoint(float x, float y)
        {
            return MakePoint(new Vector2(x, y));
        }

        public Vector2 MakePoint(Vector2 pt)
        {
            pt.x *= NegateY.x;
            pt.y *= NegateY.y;
            return pt * Settings.InversePPU;
        }

        // Applies PPU multiple but does not invert Y
        public Vector2 MakePointPPU(float x, float y)
        {
            return MakePointPPU(new Vector2(x, y));
        }

        public Vector2 MakePointPPU(Vector2 pt)
        {
            return pt * Settings.InversePPU;
        }

        public Vector2[] MakePoints(Vector2[] points)
        {
            return points.Select(p => MakePoint(p)).ToArray();
        }

        public Vector2[] MakePointsPPU(Vector2[] points)
        {
            return points.Select(p => MakePointPPU(p)).ToArray();
        }

        public float MakeRotation(float rot)
        {
            return -rot;
        }

        public bool GetIsTriggerOverridable(bool defaultValue)
        {
            if (m_IsTriggerOverride.HasValue)
            {
                return m_IsTriggerOverride.Value;
            }

            return defaultValue;
        }

        public IDisposable BeginIsTriggerOverride(GameObject go)
        {
            if (m_IsTriggerOverride.HasValue)
            {
                return null;
            }

            CustomProperty property;
            if (go.TryGetCustomPropertySafe(StringConstants.Unity_IsTrigger, out property))
            {
                m_IsTriggerOverride = property.GetValueAsBool();
                return new ScopedIsTriggerOverride(this);
            }

            return null;
        }

        public IDisposable BeginLayerIgnoreMode(LayerIgnoreMode mode)
        {
            if (mode != LayerIgnoreMode)
            {
                return new ScopedLayerIgnoreMode(this, mode);
            }

            return null;
        }

        private class ScopedIsTriggerOverride : IDisposable
        {
            private SuperImportContext m_SuperContext;

            public ScopedIsTriggerOverride(SuperImportContext superContext)
            {
                m_SuperContext = superContext;
            }

            public void Dispose()
            {
                m_SuperContext.m_IsTriggerOverride = null;
            }
        }

        private class ScopedLayerIgnoreMode : IDisposable
        {
            private SuperImportContext m_SuperContext;
            private LayerIgnoreMode m_RestoreIgnoreMode;

            public ScopedLayerIgnoreMode(SuperImportContext superContext, LayerIgnoreMode newIgnoreMode)
            {
                m_SuperContext = superContext;
                m_RestoreIgnoreMode = m_SuperContext.LayerIgnoreMode;
                m_SuperContext.LayerIgnoreMode = newIgnoreMode;
            }

            public void Dispose()
            {
                m_SuperContext.LayerIgnoreMode = m_RestoreIgnoreMode;
            }
        }
    }
}
