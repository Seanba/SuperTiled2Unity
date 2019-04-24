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
        private string m_Version = "get_rid_of_this"; // fixit - version should not be in data, right?
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
        private Material m_DefaultMaterial = null;
        public Material DefaultMaterial { get { return m_DefaultMaterial; } }

        [SerializeField]
        private TextAsset m_ObjectTypesXml = null;
        public TextAsset ObjectTypesXml { get { return m_ObjectTypesXml; } }

        [SerializeField]
        private List<Color> m_LayerColors = new List<Color>()
        {
            NamedColors.SteelBlue,          // Builtin - Default
            NamedColors.Tomato,             // Builtin - TransparentFX
            NamedColors.AliceBlue,          // Builtin - Ignore Raycast
            NamedColors.MediumPurple,
            NamedColors.PowderBlue,         // Builtin - Water
            NamedColors.DarkSeaGreen,       // Builtin - UI
            NamedColors.Khaki,
            NamedColors.IndianRed,
            NamedColors.LightGray,
            NamedColors.Yellow,
            NamedColors.SpringGreen,
            NamedColors.PaleGoldenrod,
            NamedColors.Bisque,
            NamedColors.LightSteelBlue,
            NamedColors.PeachPuff,
            NamedColors.MistyRose,
            NamedColors.MintCream,
            NamedColors.DarkRed,
            NamedColors.Silver,
            NamedColors.Orchid,
            NamedColors.DarkOrchid,
            NamedColors.DarkOliveGreen,
            NamedColors.DodgerBlue,
            NamedColors.WhiteSmoke,
            NamedColors.Honeydew,
            NamedColors.LightPink,
            NamedColors.Plum,
            NamedColors.GreenYellow,
            NamedColors.Snow,
            NamedColors.Orange,
            NamedColors.Cyan,
            NamedColors.RosyBrown,
        };

        public List<Color> LayerColors { get { return m_LayerColors; } }

        [SerializeField]
        private List<CustomObjectType> m_CustomObjectTypes;
        public List<CustomObjectType> CustomObjectTypes { get { return m_CustomObjectTypes; } }

        [SerializeField]
        private List<TypePrefabReplacement> m_PrefabReplacements = new List<TypePrefabReplacement>();
        public List<TypePrefabReplacement> PrefabReplacements { get { return m_PrefabReplacements; } }

        internal static ST2USettings GetOrCreateSettings()
        {
            var settings = AssetDatabaseEx.LoadFirstAssetByFilterAndExtension<ST2USettings>("t: ST2USettings", "asset");
            if (settings == null)
            {
                // This shouldn't often happen but we need settings in case they get deleted
                settings = CreateInstance<ST2USettings>();
                AssetDatabase.CreateAsset(settings, "Assets/SuperTiled2Unity/ST2U Settings.asset");
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        // I think almost everything below can go or change // fixit

        public void AssignSettings(SuperSettingsImporter importer) // fixit - this bites
        {
            m_Version = importer.Version;
            m_PixelsPerUnit = importer.PixelsPerUnit;
            m_EdgesPerEllipse = importer.EdgesPerEllipse;
            m_ObjectTypesXml = importer.ObjectTypesXml;
            m_AnimationFramerate = importer.AnimationFramerate;
            m_DefaultMaterial = importer.DefaultMaterial;
            m_LayerColors = importer.LayerColors;
            FillCustomObjectTypes(importer); // fixit - get rid of this
            AssignPrefabReplacements(importer); // fixit - get rid of this
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

        public void AssignPrefabReplacements(SuperSettingsImporter importer)
        {
            // Clean up the importer settings
            // Remove any prefab replacements for types that don't exist anymore
            importer.PrefabReplacements.RemoveAll(r => !m_CustomObjectTypes.Any(t => r.m_TypeName == t.m_Name));

            // Add prefab replacements for missing object types
            foreach (var cot in m_CustomObjectTypes)
            {
                if (!importer.PrefabReplacements.Any(r => cot.m_Name == r.m_TypeName))
                {
                    var rep = new TypePrefabReplacement();
                    rep.m_TypeName = cot.m_Name;
                    importer.PrefabReplacements.Add(rep);
                }
            }

            // Assign our own list of prefab replacements
            m_PrefabReplacements = importer.PrefabReplacements;
        }

        public GameObject GetPrefabReplacement(string type)
        {
            var replacement = PrefabReplacements.FirstOrDefault(r => r.m_TypeName == type);
            if (replacement != null)
            {
                return replacement.m_Prefab;
            }

            return null;
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
        private static void SelectProjectSettings() // fixit - open in project settings instead of this monstrosity
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
    }
}


