using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_2020_2_OR_NEWER
using ScriptedImporterAttribute = UnityEditor.AssetImporters.ScriptedImporterAttribute;
#else
using ScriptedImporterAttribute = UnityEditor.Experimental.AssetImporters.ScriptedImporterAttribute;
#endif

namespace SuperTiled2Unity.Editor
{
    [ScriptedImporter(ImporterConstants.TilesetVersion, ImporterConstants.TilesetExtension, ImporterConstants.TilesetImportOrder)]
    public class TsxAssetImporter : TiledAssetImporter
    {
        // Serialized data to be used in the import process
        [SerializeField]
        private bool m_UseSpriteAtlas = true;
        public bool UseSpriteAtlas { get { return m_UseSpriteAtlas; } }

        [SerializeField]
        private AtlasSize m_AtlasWidth = AtlasSize._2048;
        public int AtlasWidth { get { return (int)m_AtlasWidth; } }

        [SerializeField]
        private AtlasSize m_AtlasHeight = AtlasSize._2048;
        public int AtlasHeight { get { return (int)m_AtlasHeight; } }

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

            var icon = SuperIcons.GetTsxIcon();

            Tileset = ScriptableObject.CreateInstance<SuperTileset>();
            Tileset.m_IsInternal = false;
            Tileset.m_PixelsPerUnit = PixelsPerUnit;
            SuperImportContext.AddObjectToAsset("_TilesetScriptObject", Tileset, icon);
            SuperImportContext.SetMainObject(Tileset);

            var loader = new TilesetLoader(Tileset, this, m_UseSpriteAtlas, (int)m_AtlasWidth, (int)m_AtlasHeight);
            loader.LoadFromXml(xTileset);
        }
    }
}
