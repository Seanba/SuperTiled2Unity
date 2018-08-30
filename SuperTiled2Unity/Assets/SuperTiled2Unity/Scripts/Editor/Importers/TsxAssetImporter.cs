using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

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
            AddSuperAsset<SuperAssetTileset>();
            ImportTsxFile();
        }

        private void ImportTsxFile()
        {
            XDocument doc = XDocument.Load(this.assetPath);
            var xTileset = doc.Element("tileset");
            ProcessTileset(xTileset);
        }

        private void ProcessTileset(XElement xTileset)
        {
            CreateTileset(xTileset);
            Assert.IsNotNull(this.Tileset);
        }

        private void CreateTileset(XElement xTileset)
        {
            Assert.IsNull(this.Tileset);

            Tileset = ScriptableObject.CreateInstance<SuperTileset>();
            Tileset.m_IsInternal = false;
            SuperImportContext.AddObjectToAsset("_TilesetScriptObject", Tileset, SuperImportContext.Icons.TsxIcon);
            SuperImportContext.SetMainObject(this.Tileset);

            var loader = new TilesetLoader(this.Tileset, this, m_UseSpriteAtlas, (int)m_AtlasWidth, (int)m_AtlasHeight);
            loader.LoadFromXml(xTileset);
        }
    }
}
