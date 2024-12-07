using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public abstract class SuperImporter : ScriptedImporter
    {
        // Keep track of our importer version so that we may handle converions from old imports
        [SerializeField]
        private int m_ImporterVersion = 0;
        public int ImporterVersion
        {
            get => m_ImporterVersion;
            protected set => m_ImporterVersion = value;
        }

        // For tracking assets and dependencies imported by SuperTiled2Unity
        private SuperAsset m_SuperAsset;

        // For keeping track of errors while our asset and dependencies are being imported
        protected ImportErrors ImportErrors { get; private set; }

        protected AssetImportContext AssetImportContext { get; private set; }

        public override sealed void OnImportAsset(AssetImportContext ctx)
        {
            m_SuperAsset = null;
            ImportErrors = null;
            AssetImportContext = ctx;

#if UNITY_2020_3_OR_NEWER
            try
            {
                InternalOnImportAsset();
                InternalOnImportAssetCompleted();
            }
            catch (XmlException xml)
            {
                // Xml exceptions are common if the Tiled data file somehow becomes corrupted
                ReportGenericError($"Asset file may contain corrupted XML data. Trying opening in Tiled Map Editor to resolve.\n{xml.Message}");
            }
            catch (Exception ex)
            {
                // These errors should be reported for bug fixing
                ReportGenericError($"Unknown error encountered ({ex.GetType().Name}). Please report as a bug.\nUnknown error importing '{assetPath}'\n{ex.Message}\nStack Trace:\n{ex.StackTrace}");
            }
#else
            {
                string error = SuperTiled2Unity_Config.GetVersionError();
                Debug.LogError(error);
            }
#endif
        }

        public T RequestDependencyAssetAtPath<T>(string path) where T : UnityEngine.Object
        {
            Assert.IsNotNull(m_SuperAsset, "Must be a SuperAsset type if we are requesting dependencies.");

            // Asset is not in our cache so load it from the asset database
            string absPath;
            if (Path.IsPathRooted(path))
            {
                // Rooted (and absolute) paths baked into Tiled files are not recommended but if they resolve to a Unity asset then so be it.
                absPath = Path.GetFullPath(path);
            }
            else
            {
                // Passed-in path should be relative to this asset
                var thisAssetFolder = Path.GetDirectoryName(assetPath);
                var combinedPath = Path.Combine(thisAssetFolder, path);
                absPath = Path.GetFullPath(combinedPath);
            }

            string requestedAssetPath = absPath;
            if (!AssetPath.TryAbsoluteToAsset(ref requestedAssetPath))
            {
                ReportMissingDependency(absPath);
                return null;
            }

            // Keep track that the asset is a dependency
            // We do this even if the asset doesn't exist (as long as we have a valid asset path)
            m_SuperAsset.AddDependency(AssetImportContext, requestedAssetPath);

            // In most cases our dependency is already known by the AssetDatabase
            T asset = AssetDatabase.LoadAssetAtPath<T>(requestedAssetPath);
            if (asset != null)
            {
                // We also need to know if the dependency asset itself has errors
                var errors = AssetDatabase.LoadAssetAtPath<ImportErrors>(requestedAssetPath);
                if (errors != null)
                {
                    ReportErrorsInDependency(requestedAssetPath);
                }

                return asset;
            }
            else
            {
                ReportMissingDependency(path);
            }

            return null;
        }

        public bool CheckSortingLayerName(string sortName)
        {
            if (!sortName.Equals("Default", StringComparison.OrdinalIgnoreCase) && SortingLayer.NameToID(sortName) == 0)
            {
                ReportMissingSortingLayer(sortName);
                return false;
            }

            return true;
        }

        public bool CheckLayerName(string layerName)
        {
            if (!UnityEditorInternal.InternalEditorUtility.layers.Contains(layerName))
            {
                ReportMissingLayer(layerName);
                return false;
            }

            return true;
        }

        public bool CheckTagName(string tagName)
        {
            if (!UnityEditorInternal.InternalEditorUtility.tags.Contains(tagName))
            {
                ReportMissingTag(tagName);
                return false;
            }

            return true;
        }

        public void ReportMissingDependency(string dependencyAssetPath)
        {
            AddImportErrorsScriptableObjectIfNeeded();
            ImportErrors.ReportMissingDependency(dependencyAssetPath);
        }

        public void ReportErrorsInDependency(string dependencyAssetPath)
        {
            AddImportErrorsScriptableObjectIfNeeded();
            ImportErrors.ReportErrorsInDependency(dependencyAssetPath);
        }

        public void ReportMissingSprite(string textureAssetPath, int spriteId, int x, int y, int w, int h)
        {
            AddImportErrorsScriptableObjectIfNeeded();
            ImportErrors.ReportMissingSprite(textureAssetPath, spriteId, x, y, w, h);
        }

        public void ReportWrongTextureSize(string textureAssetPath, int expected_w, int expected_h, int actual_w, int actual_h)
        {
            AddImportErrorsScriptableObjectIfNeeded();
            ImportErrors.ReportWrongTextureSize(textureAssetPath, expected_w, expected_h, actual_w, actual_h);
        }

        public void ReportWrongPixelsPerUnit(string dependencyAssetPath, float dependencyPPU, float ourPPU)
        {
            AddImportErrorsScriptableObjectIfNeeded();
            ImportErrors.ReportWrongPixelsPerUnit(dependencyAssetPath, dependencyPPU, ourPPU);
        }

        public void ReportMissingTag(string tag)
        {
            AddImportErrorsScriptableObjectIfNeeded();
            ImportErrors.ReportMissingTag(tag);
        }

        public void ReportMissingLayer(string layer)
        {
            AddImportErrorsScriptableObjectIfNeeded();
            ImportErrors.ReportMissingLayer(layer);
        }

        public void ReportMissingSortingLayer(string sortingLayer)
        {
            AddImportErrorsScriptableObjectIfNeeded();
            ImportErrors.ReportMissingSortingLayer(sortingLayer);
        }

        public void ReportGenericError(string error)
        {
            Debug.LogError(error);
            AddImportErrorsScriptableObjectIfNeeded();
            ImportErrors.ReportGenericError(error);
        }

        private void AddImportErrorsScriptableObjectIfNeeded()
        {
            if (ImportErrors == null)
            {
                ImportErrors = ScriptableObject.CreateInstance<ImportErrors>();
                ImportErrors.name = "import-errors";
                AssetImportContext.AddObjectToAsset("_import-errors", ImportErrors);
            }
        }

        protected void AddSuperAsset<T>() where T : SuperAsset
        {
            m_SuperAsset = ScriptableObject.CreateInstance<T>();
            m_SuperAsset.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetImportContext.AddObjectToAsset("_superAsset", m_SuperAsset);
        }

        protected abstract void InternalOnImportAsset();
        protected abstract void InternalOnImportAssetCompleted();
    }
}
