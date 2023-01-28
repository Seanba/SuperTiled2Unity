using System.Xml.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // Helps us create SuperLayer objects from XML
    public abstract class SuperLayerLoader
    {
        protected XElement Xml { get; }
        protected TiledAssetImporter Importer { get; }

        protected SuperLayerLoader(XElement xml, TiledAssetImporter importer)
        {
            Xml = xml;
            Importer = importer;
        }

        public SuperLayer CreateLayer(GameObject go)
        {
            var layer = CreateLayerComponent(go);

            layer.m_TiledName = Xml.GetAttributeAs("name", "layer");
            layer.m_Visible = Xml.GetAttributeAs("visible", true);
            layer.m_OffsetX = Xml.GetAttributeAs("offsetx", 0.0f);
            layer.m_OffsetY = Xml.GetAttributeAs("offsety", 0.0f);
            layer.m_ParallaxX = Xml.GetAttributeAs("parallaxx", 1.0f);
            layer.m_ParallaxY = Xml.GetAttributeAs("parallaxy", 1.0f);
            layer.m_Opacity = Xml.GetAttributeAs("opacity", 1.0f);
            layer.m_TintColor = Xml.GetAttributeAsColor("tintcolor", Color.white);

            // Internal method will get the layer component it wants and fill out any extra data needed from the xml element.
            InternalLoadFromXml(go);

            return layer;
        }

        protected abstract SuperLayer CreateLayerComponent(GameObject go);
        protected abstract void InternalLoadFromXml(GameObject go);
    }
}
