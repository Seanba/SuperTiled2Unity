using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class SuperGroupLayerLoader : SuperLayerLoader
    {
        public SuperGroupLayerLoader(XElement xml)
            : base(xml)
        {
        }

        public override bool WorldPositionStays { get { return true; } }

        protected override SuperLayer CreateLayerComponent(GameObject go)
        {
            return go.AddComponent<SuperGroupLayer>();
        }

        protected override void InternalLoadFromXml(GameObject go)
        {
            // No extra data to load from the xml
        }
    }
}
