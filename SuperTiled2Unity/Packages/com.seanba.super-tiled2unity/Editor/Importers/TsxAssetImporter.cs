using System;
using System.Xml.Linq;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Assertions;
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
            DoCustomImporting();
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

            var loader = new TilesetLoader(Tileset, this, 0);
            loader.ColliderType = m_ColliderType;
            loader.LoadFromXml(xTileset);
        }

        private void DoCustomImporting()
        {
            foreach (var type in AutoCustomTsxImporterAttribute.GetOrderedAutoImportersTypes())
            {
                RunCustomImporterType(type);
            }
        }

        private void RunCustomImporterType(Type type)
        {
            CustomTsxImporter customImporter;
            try
            {
                customImporter = Activator.CreateInstance(type) as CustomTsxImporter;
            }
            catch (Exception e)
            {
                ReportGenericError($"Error creating custom importer class. Message = '{e.Message}'\n{e.StackTrace}");
                return;
            }

            try
            {
                var args = new TsxAssetImportedArgs();
                args.AssetImporter = this;
                args.ImportedTileset = Tileset;

                customImporter.TsxAssetImported(args);
            }
            catch (CustomImporterException cie)
            {
                ReportGenericError($"Custom Importer error: \n  Importer: {customImporter.GetType().Name}\n  Message: {cie.Message}");
                Debug.LogErrorFormat("Custom Importer ({0}) exception: {1}", customImporter.GetType().Name, cie.Message);
            }
            catch (Exception e)
            {
                ReportGenericError($"Custom importer '{customImporter.GetType().Name}' threw an exception. Message = '{e.Message}', Stack:\n{e.StackTrace}");
                Debug.LogErrorFormat("Custom importer general exception: {0}", e.Message);
            }
        }
    }
}
