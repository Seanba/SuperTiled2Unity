using System.Xml.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class SuperTileLayerLoader : SuperLayerLoader
    {
        public SuperTileLayerLoader(XElement xml, TiledAssetImporter importer)
            : base(xml, importer)
        {
        }

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
