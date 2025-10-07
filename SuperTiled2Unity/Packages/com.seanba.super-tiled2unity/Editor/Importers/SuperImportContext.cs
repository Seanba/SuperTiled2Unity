﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public class SuperImportContext
    {
        private static readonly Vector2 NegateY = new Vector2(1, -1);

        private readonly AssetImportContext m_Context;

        public float PixelsPerUnit { get; }
        public float InversePPU => 1.0f / PixelsPerUnit;
        public int EdgesPerEllipse { get; }

        public SuperImportContext(AssetImportContext context, float pixelsPerUnit, int edgesPerEllipse)
        {
            Assert.IsTrue(pixelsPerUnit > 0);
            Assert.IsTrue(edgesPerEllipse > 0);

            m_Context = context;
            PixelsPerUnit = pixelsPerUnit;
            EdgesPerEllipse = edgesPerEllipse;
        }

        public LayerIgnoreMode LayerIgnoreMode { get; private set; }
        public bool? IsTriggerOverride { get; set; }

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
            m_Context.GetObjects(objects);
            return objects.Count;
        }

        // Math/space transform functions
        // Points in Tiled have (0, 0) at top left corner of map (+y goes down)
        // Our Unity projects have +y going up and points are transformed by a Pixels Per Unity constant
        public float MakeScalar(float s)
        {
            return s * InversePPU;
        }

        // Does not negate y
        public Vector2 MakeSize(float x, float y)
        {
            return MakeSize(new Vector2(x, y));
        }

        public Vector2 MakeSize(Vector2 size)
        {
            return size * InversePPU;
        }

        public Vector2 MakePoint(float x, float y)
        {
            return MakePoint(new Vector2(x, y));
        }

        public Vector2 MakePoint(Vector2 pt)
        {
            pt.x *= NegateY.x;
            pt.y *= NegateY.y;
            return pt * InversePPU;
        }

        // Applies PPU multiple but does not invert Y
        public Vector2 MakePointPPU(float x, float y)
        {
            return MakePointPPU(new Vector2(x, y));
        }

        public Vector2 MakePointPPU(Vector2 pt)
        {
            return pt * InversePPU;
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
            if (IsTriggerOverride != null)
            {
                return IsTriggerOverride.Value;
            }

            return defaultValue;
        }

        public IDisposable BeginIsTriggerOverride(GameObject go)
        {
            if (go.TryGetCustomPropertySafe(StringConstants.Unity_IsTrigger, out CustomProperty property))
            {
                return new ScopedIsTriggerOverride(this, property.GetValueAsBool());
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
            private readonly SuperImportContext m_SuperContext;
            private readonly bool? m_RestoreIsTriggerOverride;

            public ScopedIsTriggerOverride(SuperImportContext superContext, bool isTriggerOverride)
            {
                m_SuperContext = superContext;
                m_RestoreIsTriggerOverride = m_SuperContext.IsTriggerOverride;
                m_SuperContext.IsTriggerOverride = isTriggerOverride;
            }

            public void Dispose()
            {
                m_SuperContext.IsTriggerOverride = m_RestoreIsTriggerOverride;
            }
        }

        private class ScopedLayerIgnoreMode : IDisposable
        {
            private readonly SuperImportContext m_SuperContext;
            private readonly LayerIgnoreMode m_RestoreIgnoreMode;

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
