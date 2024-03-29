using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor.AssetImporters;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity.Editor
{
    [ScriptedImporter(ImporterConstants.TilesetVersion, ImporterConstants.TilesetExtension, ImporterConstants.TilesetImportOrder)]
    public class TsxAssetImporter : TiledAssetImporter
    {
        public const string ColliderTypeSerializedName = nameof(m_ColliderType);

        public Tile.ColliderType m_ColliderType;

        public SuperTileset Tileset { get; private set; }

        protected override void InternalOnImportAsset()
        {
            base.InternalOnImportAsset();

            ImporterVersion = ImporterConstants.TilesetVersion;

            AddSuperAsset<SuperAssetTileset>();
            ImportTsxFile();
        }

        private void ImportTsxFile()
        {
            XDocument doc = XDocument.Load(assetPath);
            var xTileset = doc.Element("tileset");
            ProcessTileset(xTileset);
        }

        private void ProcessTileset(XElement xTileset)
        {
            CreateTileset(xTileset);
            Assert.IsNotNull(Tileset);
        }

        private void CreateTileset(XElement xTileset)
        {
            Assert.IsNull(Tileset);

            var icon = SuperIcons.instance.m_TsxIcon;

            Tileset = ScriptableObject.CreateInstance<SuperTileset>();
            Tileset.m_IsInternal = false;
            Tileset.m_PixelsPerUnit = PixelsPerUnit;
            SuperImportContext.AddObjectToAsset("_TilesetScriptObject", Tileset, icon);
            SuperImportContext.SetMainObject(Tileset);

            var loader = new TilesetLoader(Tileset, this);
            loader.LoadFromXml(xTileset);
        }
    }
}
