using System.Xml.Linq;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Assertions;

// Importer for Tiled TX (template) files
namespace SuperTiled2Unity.Editor
{
    [ScriptedImporter(ImporterConstants.TemplateVersion, ImporterConstants.TemplateExtension, ImporterConstants.TemplateImportOrder)]
    public class TxAssetImporter : SuperImporter
    {
        private ObjectTemplate m_ObjectTemplate;
        private GlobalTileDatabase m_GlobalTileDatabase;

        protected override void InternalOnImportAsset()
        {
            var icon = AssetDatabaseEx.LoadFirstAssetByFilter<Texture2D>("tx-file-icon-0x1badd00d");
            AddSuperAsset<SuperAssetTemplate>();

            // Create our asset and build it out
            m_ObjectTemplate = ScriptableObject.CreateInstance<ObjectTemplate>();
            AssetImportContext.AddObjectToAsset("_template", m_ObjectTemplate, icon);
            AssetImportContext.SetMainObject(m_ObjectTemplate);

            // Tx files are XML that contain the data for one object
            XDocument doc = XDocument.Load(this.assetPath);
            var xTemplate = doc.Element("template");
            ProcessTemplate(xTemplate);
        }

        protected override void InternalOnImportAssetCompleted()
        {
        }

        private void ProcessTemplate(XElement xTemplate)
        {
            // Do we have a tileset?
            var xTileset = xTemplate.Element("tileset");
            if (xTileset != null)
            {
                ProcessTileset(xTileset);
            }

            var xObject = xTemplate.Element("object");
            if (xObject != null)
            {
                ProcessObject(xObject);
            }
            else
            {
                ReportError("Template file does not contain an object element.");
            }
        }

        private void ProcessTileset(XElement xTileset)
        {
            var firstId = xTileset.GetAttributeAs<int>("firstgid");
            var source = xTileset.GetAttributeAs<string>("source");

            // Load the tileset and process the tiles inside
            var tileset = RequestAssetAtPath<SuperTileset>(source);

            if (tileset == null)
            {
                // Tileset is either missing or is not yet ready
                ReportError("Missing tileset asset: {0}", this.assetPath);
            }
            else
            {
                // Register all the tiles with the tile database for this map
                m_GlobalTileDatabase = new GlobalTileDatabase();
                m_GlobalTileDatabase.RegisterTileset(firstId, tileset);
            }
        }

        private void ProcessObject(XElement xObject)
        {
            var objectXml = xObject.ToString();
            m_ObjectTemplate.m_ObjectXml = objectXml;

            ProcessTile(xObject);
            ProcessProperties(xObject);
        }

        private void ProcessTile(XElement xObject)
        {
            uint gId = xObject.GetAttributeAs<uint>("gid", 0);
            if (gId != 0)
            {
                Assert.IsNotNull(m_GlobalTileDatabase);
                var tileId = new TileIdMath(gId);

                // Store a reference to the tile
                SuperTile tile;
                if (m_GlobalTileDatabase.TryGetTile(tileId.JustTileId, out tile))
                {
                    m_ObjectTemplate.m_Tile = tile;
                }
                else
                {
                    ReportError("Could not find tile '{0}' in tileset", tileId.JustTileId);
                }
            }
        }

        private void ProcessProperties(XElement xObject)
        {
            var xProperties = xObject.Element("properties");
            if (xProperties != null)
            {
                var properties = CustomPropertyLoader.LoadCustomPropertyList(xProperties);
                m_ObjectTemplate.m_CustomProperties = properties;
            }
        }
    }
}
