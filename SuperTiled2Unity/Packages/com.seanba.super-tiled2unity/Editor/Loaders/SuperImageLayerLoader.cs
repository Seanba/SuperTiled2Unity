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
            // No extra data to load from the xml
        }
    }
}
