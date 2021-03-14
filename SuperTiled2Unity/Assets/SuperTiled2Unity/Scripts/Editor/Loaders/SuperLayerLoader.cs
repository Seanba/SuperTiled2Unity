using System.Xml.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // Helps us create SuperLayer objects from XML
    public abstract class SuperLayerLoader
    {
        protected XElement m_Xml;

        protected SuperLayerLoader(XElement xml)
        {
            m_Xml = xml;
        }

        public SuperLayer CreateLayer(GameObject go)
        {
            var layer = CreateLayerComponent(go);

            layer.m_TiledName = m_Xml.GetAttributeAs("name", "layer");
            layer.m_Visible = m_Xml.GetAttributeAs("visible", true);
            layer.m_OffsetX = m_Xml.GetAttributeAs("offsetx", 0.0f);
            layer.m_OffsetY = m_Xml.GetAttributeAs("offsety", 0.0f);
            layer.m_Opacity = m_Xml.GetAttributeAs("opacity", 1.0f);
            layer.m_TintColor = m_Xml.GetAttributeAsColor("tintcolor", Color.white);

            // Internal method will get the layer component it wants and fill out any extra data needed from the xml element.
            InternalLoadFromXml(go);

            return layer;
        }

        protected abstract SuperLayer CreateLayerComponent(GameObject go);
        protected abstract void InternalLoadFromXml(GameObject go);
    }
}
