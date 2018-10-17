using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class ST2USettings : ScriptableObject
    {
        [SerializeField]
        private string m_Version = "unknown";
        public string Version { get { return m_Version; } }

        [SerializeField]
        private float m_PixelsPerUnit = 100.0f;
        public float PixelsPerUnit { get { return m_PixelsPerUnit; } }
        public float InversePPU { get { return 1.0f / PixelsPerUnit; } }

        [SerializeField]
        private int m_EdgesPerEllipse = 32;
        public int EdgesPerEllipse { get { return m_EdgesPerEllipse; } }

        [SerializeField]
        private int m_AnimationFramerate = 20;
        public int AnimationFramerate {  get { return m_AnimationFramerate; } }

        [SerializeField]
        private TextAsset m_ObjectTypesXml = null;
        public TextAsset ObjectTypesXml { get { return m_ObjectTypesXml; } }

        [SerializeField]
        private List<Color> m_LayerColors = new List<Color>(Enumerable.Repeat(NamedColors.LightSteelBlue, 32));
        public List<Color> LayerColors { get { return m_LayerColors; } }

        [SerializeField]
        private List<CustomObjectType> m_CustomObjectTypes;
        public List<CustomObjectType> CustomObjectTypes { get { return m_CustomObjectTypes; } }

        public void AssignSettings(SuperSettingsImporter importer)
        {
            m_Version = importer.Version;
            m_PixelsPerUnit = importer.PixelsPerUnit;
            m_EdgesPerEllipse = importer.EdgesPerEllipse;
            m_ObjectTypesXml = importer.ObjectTypesXml;
            m_AnimationFramerate = importer.AnimationFramerate;
            m_LayerColors = importer.LayerColors;
            FillCustomObjectTypes(importer);
        }

        public void DefaultOrOverride_PixelsPerUnit(ref float ppu)
        {
            if (ppu == 0)
            {
                ppu = Clamper.ClampPixelsPerUnit(m_PixelsPerUnit);
            }
            else
            {
                m_PixelsPerUnit = Clamper.ClampPixelsPerUnit(ppu);
            }
        }

        public void DefaultOrOverride_EdgesPerEllipse(ref int edgesPerEllipse)
        {
            if (edgesPerEllipse == 0)
            {
                edgesPerEllipse = Clamper.ClampEdgesPerEllipse(m_EdgesPerEllipse);
            }
            else
            {
                m_EdgesPerEllipse = Clamper.ClampEdgesPerEllipse(edgesPerEllipse);
            }
        }

        private void FillCustomObjectTypes(SuperSettingsImporter importer)
        {
            m_CustomObjectTypes = new List<CustomObjectType>();

            if (m_ObjectTypesXml != null)
            {
                XDocument xdoc = XDocument.Parse(m_ObjectTypesXml.text);

                if (xdoc.Root.Name != "objecttypes")
                {
                    importer.ReportError("'{0}' is not a valid object types xml file.", m_ObjectTypesXml.name);
                    return;
                }

                // Create a dependency on our Object Types xml file so that settings are automatically updated if it changes
                importer.AddAssetPathDependency(AssetDatabase.GetAssetPath(m_ObjectTypesXml));

                // Import the data from the objecttype elements
                foreach (var xObjectType in xdoc.Descendants("objecttype"))
                {
                    var cot = new CustomObjectType();
                    cot.m_Name = xObjectType.GetAttributeAs("name", "NoName");
                    cot.m_Color = xObjectType.GetAttributeAsColor("color", Color.gray);
                    cot.m_CustomProperties = CustomPropertyLoader.LoadCustomPropertyList(xObjectType);

                    m_CustomObjectTypes.Add(cot);
                }
            }
        }

        public static ST2USettings LoadSettings()
        {
            // Find the first ST2U setting asset
            const string search = "t:ST2USettings";
            return AssetDatabaseEx.LoadFirstAssetByFilter<ST2USettings>(search);
        }

        public static SuperIcons LoadIcons()
        {
            const string search = "t:SuperIcons";
            return AssetDatabaseEx.LoadFirstAssetByFilter<SuperIcons>(search);
        }

        [MenuItem("Edit/Project Settings/SuperTiled2Unity Settings", false)]
        private static void SelectProjectSettings()
        {
            var asset = LoadSettings();
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorUtility.FocusProjectWindow();
                EditorGUIUtility.PingObject(asset);
            }
            else
            {
                Debug.LogWarningFormat("SuperTiled2Unity settings asset not found. Was it deleted? Please reinstall Super Tiled2Unity.");
            }
        }

        // This is only invoked by a deployment batch file
        private static void DeploySuperTiled2Unity()
        {
            var settings = ST2USettings.LoadSettings();
            var path = string.Format("{0}/../../deploy/SuperTiled2Unity.{1}.unitypackage", Application.dataPath, settings.Version);

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            AssetDatabase.ExportPackage("Assets/SuperTiled2Unity", path, ExportPackageOptions.Recurse);
        }
    }
}


