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
        public const string ProjectSettingsPath = "Project/SuperTiled2Unity";

        [SerializeField]
        private float m_PixelsPerUnit = 100.0f;
        public float PixelsPerUnit
        {
            get { return m_PixelsPerUnit; }
            set { m_PixelsPerUnit = value; }
        }

        [SerializeField]
        private int m_EdgesPerEllipse = 32;
        public int EdgesPerEllipse
        {
            get { return m_EdgesPerEllipse; }
            set { m_EdgesPerEllipse = value; }
        }

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
        private string m_ParseXmlError = string.Empty;
        public string ParseXmlError { get { return m_ParseXmlError; } }

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


        public float InversePPU { get { return 1.0f / PixelsPerUnit; } }

        internal static ST2USettings GetOrCreateST2USettings()
        {
            var settings = AssetDatabaseEx.LoadFirstAssetByFilterAndExtension<ST2USettings>("t: ST2USettings", "asset");
            if (settings == null)
            {
                // This shouldn't often happen but we need settings in case they get deleted on us
                settings = SuperTiled2Unity_Config.CreateDefaultSettings();
            }

            return settings;
        }

        // Invoke this to ensure that Xml Object Types are up-to-date
        // Our importers that depend on Object Types from tiled will want to call this early in their import process
        internal void RefreshCustomObjectTypes()
        {
            m_CustomObjectTypes = new List<CustomObjectType>();
            m_ParseXmlError = string.Empty;

            if (m_ObjectTypesXml != null)
            {
                try
                {
                    XDocument xdoc = XDocument.Parse(m_ObjectTypesXml.text);

                    if (xdoc.Root.Name != "objecttypes")
                    {
                        m_ParseXmlError = string.Format("'{0}' is not a valid object types xml file.", m_ObjectTypesXml.name);
                    }

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
                catch (XmlException xe)
                {
                    m_ParseXmlError = string.Format("'{0}' is not a valid XML file.\n\nError: {1}", m_ObjectTypesXml.name, xe.Message);
                    m_CustomObjectTypes.Clear();
                }
                catch (Exception e)
                {
                    m_ParseXmlError = e.Message;
                    m_CustomObjectTypes.Clear();
                }
            }
        }

        public void SortPrefabReplacements()
        {
            m_PrefabReplacements = m_PrefabReplacements.OrderBy(p => p.m_TypeName).ToList();
        }

        public void AddObjectsToPrefabReplacements()
        {
            RefreshCustomObjectTypes();
            foreach (var cot in m_CustomObjectTypes)
            {
                if (!PrefabReplacements.Any(c => string.Equals(c.m_TypeName, cot.m_Name, StringComparison.OrdinalIgnoreCase)))
                {
                    PrefabReplacements.Add(new TypePrefabReplacement { m_TypeName = cot.m_Name });
                }
            }
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
    }
}


