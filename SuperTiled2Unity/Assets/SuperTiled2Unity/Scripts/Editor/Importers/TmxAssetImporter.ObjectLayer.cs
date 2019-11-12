using System.Xml.Linq;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    partial class TmxAssetImporter
    {
        private SuperLayer ProcessObjectLayer(GameObject goParent, XElement xObjectLayer)
        {
            // Have our super object layer loader take care of things
            var loader = new SuperObjectLayerLoader(xObjectLayer);
            loader.AnimationFramerate = SuperImportContext.Settings.AnimationFramerate;
            loader.ColliderFactory = CreateColliderFactory();
            loader.SuperMap = m_MapComponent;
            loader.Importer = this;
            loader.GlobalTileDatabase = m_GlobalTileDatabase;

            // Create our layer and objects
            var objectLayer = goParent.AddSuperLayerGameObject<SuperObjectLayer>(loader, SuperImportContext);
            AddSuperCustomProperties(objectLayer.gameObject, xObjectLayer.Element("properties"));

            RendererSorter.BeginObjectLayer(objectLayer);

            using (SuperImportContext.BeginIsTriggerOverride(objectLayer.gameObject))
            {
                loader.CreateObjects();
            }

            RendererSorter.EndObjectLayer(objectLayer);

            return objectLayer;
        }

        private ColliderFactory CreateColliderFactory()
        {
            if (m_MapComponent.m_Orientation == MapOrientation.Isometric)
            {
                return new ColliderFactoryIsometric(m_MapComponent.m_TileWidth, m_MapComponent.m_TileHeight, SuperImportContext);
            }
            else
            {
                return new ColliderFactoryOrthogonal(SuperImportContext);
            }
        }
    }
}
