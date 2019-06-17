using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class SuperTileLayerLoader : SuperLayerLoader
    {
        public SuperTileLayerLoader(XElement xml)
            : base(xml)
        {
        }

        public override bool WorldPositionStays { get { return false; } }

        protected override SuperLayer CreateLayerComponent(GameObject go)
        {
            return go.AddComponent<SuperTileLayer>();
        }

        protected override void InternalLoadFromXml(GameObject go)
        {
            // No extra data to load from the xml
        }
    }
}
