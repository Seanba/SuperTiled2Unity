using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    [FilePath("ProjectSettings/SuperTiled2Unity.asset", FilePathAttribute.Location.ProjectFolder)]
    public class ST2USettings : ScriptableSingleton<ST2USettings>
    {
        public float m_DefaultPixelsPerUnit = 100.0f;
        public int m_DefaultEdgesPerEllipse = 32;
        public int m_AnimationFramerate = 20;
        public Material m_DefaultMaterial = null;
        public List<LayerMaterialMatch> m_MaterialMatchings = new List<LayerMaterialMatch>();
        public TextAsset m_ObjectTypesXml = null;
        public TextAsset m_ObjectTypesJson = null;
        public string m_ParseXmlError = string.Empty;
        public CompositeCollider2D.GeometryType m_CollisionGeometryType = CompositeCollider2D.GeometryType.Polygons;

        public List<Color> m_LayerColors = new List<Color>()
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

        public List<CustomObjectType> m_CustomObjectTypes;
        public List<CustomEnumType> m_CustomEnumTypes;

        public List<TypePrefabReplacement> m_PrefabReplacements = new List<TypePrefabReplacement>();

        internal void RefreshCustomObjectTypes()
        {
            m_CustomObjectTypes = new List<CustomObjectType>();
            m_CustomEnumTypes = new List<CustomEnumType>();
            m_ParseXmlError = string.Empty;

            if (m_ObjectTypesXml != null)
            {
                RefreshObjectTypesXml();
            }

            if (m_ObjectTypesJson != null)
            {
                RefreshObjectTypesJson();
            }
        }

        internal void RegisterCustomTypeDependencies(UnityEditor.AssetImporters.AssetImportContext context)
        {
            RegisterTextAssetDependency(context, m_ObjectTypesXml);
            RegisterTextAssetDependency(context, m_ObjectTypesJson);
        }

        private void RefreshObjectTypesXml()
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
                    var cot = new CustomObjectType
                    {
                        m_Name = xObjectType.GetAttributeAs("name", "NoName"),
                        m_Color = xObjectType.GetAttributeAsColor("color", Color.gray),
                        m_CustomProperties = CustomPropertyLoader.LoadCustomPropertyList(xObjectType)
                    };

                    AddOrMergeCustomObjectType(cot);
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

        private void RefreshObjectTypesJson()
        {
            var customTypes = TiledCustomTypesJson.LoadCustomObjectTypes(m_ObjectTypesJson.text);
            var customEnumTypes = TiledCustomTypesJson.LoadCustomEnumTypes(m_ObjectTypesJson.text);
            if (customTypes.Count == 0 && customEnumTypes.Count == 0 && !string.IsNullOrWhiteSpace(m_ObjectTypesJson.text))
            {
                m_ParseXmlError = string.Format("'{0}' does not contain Tiled custom class or enum definitions.", m_ObjectTypesJson.name);
            }

            TiledCustomTypesJson.MergeInto(m_CustomObjectTypes, customTypes);
            MergeCustomEnumTypes(customEnumTypes);
        }

        private void AddOrMergeCustomObjectType(CustomObjectType customObjectType)
        {
            if (m_CustomObjectTypes.TryGetCustomObjectType(customObjectType.m_Name, out var existingType))
            {
                existingType.m_CustomProperties.CombineFromSource(customObjectType.m_CustomProperties);
            }
            else
            {
                m_CustomObjectTypes.Add(customObjectType);
            }
        }

        private void MergeCustomEnumTypes(List<CustomEnumType> customEnumTypes)
        {
            foreach (var customEnumType in customEnumTypes)
            {
                if (!m_CustomEnumTypes.TryGetCustomEnumType(customEnumType.m_Name, out _))
                {
                    m_CustomEnumTypes.Add(customEnumType);
                }
            }
        }

        private static void RegisterTextAssetDependency(UnityEditor.AssetImporters.AssetImportContext context, TextAsset textAsset)
        {
            if (context == null || textAsset == null)
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(textAsset);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            context.DependsOnSourceAsset(path);
            context.DependsOnArtifact(path);
        }

        internal void SortMaterialMatchings()
        {
            m_MaterialMatchings = m_MaterialMatchings.OrderBy(m => m.m_LayerName).ToList();
        }

        internal void SortPrefabReplacements()
        {
            m_PrefabReplacements = m_PrefabReplacements.OrderBy(p => p.m_TypeName).ToList();
        }

        internal void AddObjectsToPrefabReplacements()
        {
            RefreshCustomObjectTypes();
            foreach (var cot in m_CustomObjectTypes)
            {
                if (!m_PrefabReplacements.Any(c => string.Equals(c.m_TypeName, cot.m_Name, StringComparison.OrdinalIgnoreCase)))
                {
                    m_PrefabReplacements.Add(new TypePrefabReplacement { m_TypeName = cot.m_Name });
                }
            }
        }

        internal GameObject GetPrefabReplacement(string type)
        {
            var replacement = m_PrefabReplacements.FirstOrDefault(r => r.m_TypeName == type);
            if (replacement != null)
            {
                return replacement.m_Prefab;
            }

            return null;
        }

        internal void SaveSettings()
        {
            Save(true);
        }
    }
}


