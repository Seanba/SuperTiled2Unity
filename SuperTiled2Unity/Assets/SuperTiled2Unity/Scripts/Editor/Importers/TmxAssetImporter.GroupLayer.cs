using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public partial class TmxAssetImporter
    {
        private GameObject ProcessGroupLayer(GameObject goParent, XElement xGroup)
        {
            var groupLayerComponent = goParent.AddSuperLayerGameObject<SuperGroupLayer>(new SuperGroupLayerLoader(xGroup), SuperImportContext);
            AddSuperCustomProperties(groupLayerComponent.gameObject, xGroup.Element("properties"));

            m_LayerSorterHelper.SortNewLayer(groupLayerComponent);

            // Group layers can contain other layers
            ProcessMapLayers(groupLayerComponent.gameObject, xGroup);

            return groupLayerComponent.gameObject;
        }
    }
}
