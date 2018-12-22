using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor.Experimental.AssetImporters;

namespace SuperTiled2Unity.Editor
{
    // Note: Our settings is set to be imported first
    [ScriptedImporter(ImporterConstants.SettingsVersion, ImporterConstants.SettingsExtension, ImporterConstants.SettingsImportOrder)]
    public class SuperSettingsImporter : SuperImporter
    {
        [SerializeField]
        private string m_Version;
        public string Version { get { return m_Version; } }

        [SerializeField]
        private float m_PixelsPerUnit = 100.0f;
        public float PixelsPerUnit { get { return m_PixelsPerUnit; } }

        [SerializeField]
        private int m_EdgesPerEllipse = 32;
        public int EdgesPerEllipse { get { return m_EdgesPerEllipse; } }

        [SerializeField]
        private int m_AnimationFramerate = 20;
        public int AnimationFramerate { get { return m_AnimationFramerate; } }

        [SerializeField]
        private Material m_DefaultMaterial = null;
        public Material DefaultMaterial { get { return m_DefaultMaterial; } }

        [SerializeField]
        private TextAsset m_ObjectTypesXml = null;
        public TextAsset ObjectTypesXml { get { return m_ObjectTypesXml; } }

        private SuperIcons m_SuperIcons;

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

        protected override sealed void InternalOnImportAsset()
        {
            AddSuperAsset<SuperAssetSettings>();
            DoImportIcons();
            DoImportSettings();
        }

        private void DoImportIcons()
        {
            m_SuperIcons = ScriptableObject.CreateInstance<SuperIcons>();
            m_SuperIcons.name = "Icons";
            m_SuperIcons.AssignIcons();
            AssetImportContext.AddObjectToAsset("_icons", m_SuperIcons);
        }

        private void DoImportSettings()
        {
            Assert.IsNotNull(m_SuperIcons);

            // The asset file only contains our version number
            // The rest of the settings data will be saved out to a meta file
            m_Version = File.ReadAllText(assetPath);

            // Create asset to be used by our import context and to serve as our singleton instance
            var asset = ScriptableObject.CreateInstance<ST2USettings>();
            asset.AssignSettings(this);

            AssetImportContext.AddObjectToAsset("_main", asset, m_SuperIcons.SettingsIcon);
            AssetImportContext.SetMainObject(asset);
        }
    }
}
