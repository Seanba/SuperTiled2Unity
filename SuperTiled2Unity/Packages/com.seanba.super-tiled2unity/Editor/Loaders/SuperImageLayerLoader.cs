using System.Xml.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class SuperImageLayerLoader : SuperLayerLoader
    {
        public SuperImageLayerLoader(XElement xml, TiledAssetImporter importer)
            : base(xml, importer)
        {
        }

        protected override SuperLayer CreateLayerComponent(GameObject go)
        {
            return go.AddComponent<SuperImageLayer>();
        }

        protected override void InternalLoadFromXml(GameObject go)
        {
            var layer = go.GetComponent<SuperImageLayer>();

            layer.m_RepeatX = Xml.GetAttributeAs("repeatx", false);
            layer.m_RepeatY = Xml.GetAttributeAs("repeaty", false);
        }
    }
}
