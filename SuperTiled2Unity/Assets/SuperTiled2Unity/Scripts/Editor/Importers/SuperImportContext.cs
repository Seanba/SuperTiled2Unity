using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;

namespace SuperTiled2Unity.Editor
{
    public class SuperImportContext
    {
        private static Vector2 NegateY = new Vector2(1, -1);

        private AssetImportContext m_Context;

        public SuperImportContext(AssetImportContext context, ST2USettings settings, SuperIcons icons)
        {
            m_Context = context;
            Settings = settings;
            Icons = icons;
        }

        public ST2USettings Settings { get; private set; }
        public SuperIcons Icons { get; private set; }

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

        public Vector2[] MakePoints(Vector2[] points)
        {
            return points.Select(p => MakePoint(p)).ToArray();
        }

        public float MakeRotation(float rot)
        {
            return -rot;
        }
    }
}
