using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public abstract class SuperImporter : ScriptedImporter
    {
        [SerializeField]
        private List<string> m_Errors = new List<string>();
        public IEnumerable<string> Errors { get { return m_Errors; } }

        [SerializeField]
        private List<string> m_Warnings = new List<string>();
        public IEnumerable<string> Warnings { get { return m_Warnings; } }

        [SerializeField]
        private List<string> m_MissingSortingLayers = new List<string>();
        public IEnumerable<string> MissingSortingLayers { get { return m_MissingSortingLayers; } }

        [SerializeField]
        private List<string> m_MissingLayers = new List<string>();
        public IEnumerable<string> MissingLayers { get { return m_MissingLayers; } }

        // Keep track of loaded database objects by type
        private Dictionary<KeyValuePair<string, Type>, UnityEngine.Object> m_CachedDatabase = new Dictionary<KeyValuePair<string, Type>, UnityEngine.Object>();

        // For tracking assets and dependencies imported by SuperTiled2Unity
        private SuperAsset m_SuperAsset;

        protected AssetImportContext AssetImportContext { get; private set; }

        public override sealed void OnImportAsset(AssetImportContext ctx)
        {
            m_CachedDatabase.Clear();
            m_Errors.Clear();
            m_Warnings.Clear();
            m_MissingSortingLayers.Clear();
            m_MissingLayers.Clear();
            m_SuperAsset = null;
            AssetImportContext = ctx;

#if UNITY_2018_2_OR_NEWER
            try
            {
                InternalOnImportAsset();
                InternalOnImportAssetCompleted();
            }
            catch (TiledException tiled)
            {
                // Exceptions that SuperTiled2Unity is aware of
                // These are the kind of errors a user should be able to fix
                m_Errors.Add(tiled.Message);
            }
            catch (XmlException xml)
            {
                // Xml exceptions are common if the Tiled data file somehow becomes corrupted
                m_Errors.Add("Asset file may contained corrupted XML data. Trying opening in Tiled Map Editor to resolve.");
                m_Errors.Add(xml.Message);
            }
            catch (Exception ex)
            {
                // Last-chance collection of unknown errors while importing
                // This should be reported for bug fixing
                m_Errors.Add("Unknown error encountered. Please report as bug. Stack track is in the console output.");
                m_Errors.Add(ex.Message);
                Debug.LogErrorFormat("Unknown error of type {0}: {1}\nStack Trace:\n{2}", ex.GetType(), ex.Message, ex.StackTrace);
            }
#else
            ReportUnityVersionError();
#endif
        }

        public void AddAssetPathDependency(string assetPath)
        {
            m_SuperAsset.AddDependency(AssetImportContext, assetPath);
        }

        public T RequestAssetAtPath<T>(string path) where T : UnityEngine.Object
        {
            Assert.IsNotNull(m_SuperAsset, "Must be a SuperAsset type if we are requesting dependencies");

            // Is the asset in our cache?
            var key = new KeyValuePair<string, Type>(path.ToLower(), typeof(T));
            UnityEngine.Object cachedObject;

            if (m_CachedDatabase.TryGetValue(key, out cachedObject))
            {
                return cachedObject as T;
            }

            // The path is either relative to our asset path or is absolute
            using (new ChDir(this.assetPath))
            {
                path = Path.GetFullPath(path);
                if (AssetPath.TryAbsoluteToAsset(ref path))
                {
                    // Keep track that the the asset is a dependency
                    m_SuperAsset.AddDependency(AssetImportContext, path);

                    // In most cases our dependency is already known by the AssetDatabase
                    T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                    if (asset != null)
                    {
                        // Add the asset to our cache for next time it is requested
                        m_CachedDatabase[key] = asset;
                        return asset;
                    }
                }
            }

            return null;
        }

        public void ReportError(string fmt, params object[] args)
        {
            string error = string.Format(fmt, args);
            m_Errors.Add(error);
        }

        public void ReportWarning(string fmt, params object[] args)
        {
            string warning = string.Format(fmt, args);
            m_Warnings.Add(warning);
        }

        public bool CheckSortingLayerName(string sortName)
        {
            if (!sortName.Equals("Default", StringComparison.OrdinalIgnoreCase) && SortingLayer.NameToID(sortName) == 0)
            {
                if (!m_MissingSortingLayers.Contains(sortName))
                {
                    //Debug.LogWarningFormat("Sorting layer name '{0}' not found in Tag Manager. Tiled map layers and objects may be drawn out of order.", sortName);
                    m_MissingSortingLayers.Add(sortName);
                }

                return false;
            }

            return true;
        }

        public bool CheckLayerName(string layerName)
        {
            if (LayerMask.NameToLayer(layerName) == -1)
            {
                if (!m_MissingLayers.Contains(layerName))
                {
                    //Debug.LogWarningFormat("Layer name '{0}' not found in Tag Manager. Colliders may not work as expected", layerName);
                    m_MissingLayers.Add(layerName);
                }

                return false;
            }

            return true;
        }

        protected void AddSuperAsset<T>() where T : SuperAsset
        {
            m_SuperAsset = ScriptableObject.CreateInstance<T>();
            m_SuperAsset.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetImportContext.AddObjectToAsset("_superAsset", m_SuperAsset);
        }

        protected abstract void InternalOnImportAsset();
        protected abstract void InternalOnImportAssetCompleted();

        private void ReportUnityVersionError()
        {
            string error = string.Format("SuperTiled2Unity requires Unity 2018.2 or later. You are using {0}", Application.unityVersion);
            ReportError(error);
            Debug.LogError(error);
        }
    }
}
