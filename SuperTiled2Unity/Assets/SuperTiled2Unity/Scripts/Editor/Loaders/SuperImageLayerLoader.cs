using System.Xml.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class SuperImageLayerLoader : SuperLayerLoader
    {
        public SuperImageLayerLoader(XElement xml)
            : base(xml)
        {
        }

        protected override SuperLayer CreateLayerComponent(GameObject go)
        {
            return go.AddComponent<SuperImageLayer>();
        }

        protected override void InternalLoadFromXml(GameObject go)
        {
            var layer = go.GetComponent<SuperImageLayer>();

            layer.m_RepeatX = m_Xml.GetAttributeAs("repeatx", false);
            layer.m_RepeatY = m_Xml.GetAttributeAs("repeaty", false);
        }
    }
}
