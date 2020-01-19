using System.Xml.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public partial class TmxAssetImporter
    {
        private SuperLayer ProcessGroupLayer(GameObject goParent, XElement xGroup)
        {
            var groupLayerComponent = goParent.AddSuperLayerGameObject<SuperGroupLayer>(new SuperGroupLayerLoader(xGroup), SuperImportContext);
            AddSuperCustomProperties(groupLayerComponent.gameObject, xGroup.Element("properties"));

            // Group layers can contain other layers
            RendererSorter.BeginGroupLayer(groupLayerComponent);

            using (SuperImportContext.BeginIsTriggerOverride(groupLayerComponent.gameObject))
            {
                ProcessMapLayers(groupLayerComponent.gameObject, xGroup);
            }

            RendererSorter.EndGroupLayer();

            return groupLayerComponent;
        }
    }
}
