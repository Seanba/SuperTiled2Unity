using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace SuperTiled2Unity.Editor
{
    [ScriptedImporter(ImporterConstants.TilesetVersion, ImporterConstants.TilesetExtension, ImporterConstants.TilesetImportOrder)]
    public class TsxAssetImporter : TiledAssetImporter
    {
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

            var loader = new TilesetLoader(this.Tileset, this);
            loader.LoadFromXml(xTileset);
        }
    }
}
