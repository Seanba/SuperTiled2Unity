using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // Collection of icons to be used with SuperTiled2Unity assets
    public class SuperIcons : ScriptableObject
    {
        [SerializeField]
        private Texture2D m_SettingsIcon;
        public Texture2D SettingsIcon { get { return m_SettingsIcon; } }

        [SerializeField]
        private Texture2D m_TmxIcon;
        public Texture2D TmxIcon { get { return m_TmxIcon; } }

        [SerializeField]
        private Texture2D m_TsxIcon;
        public Texture2D TsxIcon { get { return m_TsxIcon; } }

        [SerializeField]
        private Texture2D m_TxIcon;
        public Texture2D TxIcon { get { return m_TxIcon; } }

        public void AssignIcons()
        {
            m_SettingsIcon = AssetDatabaseEx.LoadFirstAssetByFilter<Texture2D>("tiled-settings-icon-0x1badd00d");
            m_TmxIcon = AssetDatabaseEx.LoadFirstAssetByFilter<Texture2D>("tmx-file-icon-0x1badd00d");
            m_TsxIcon = AssetDatabaseEx.LoadFirstAssetByFilter<Texture2D>("tsx-file-icon-0x1badd00d");
            m_TxIcon = AssetDatabaseEx.LoadFirstAssetByFilter<Texture2D>("tx-file-icon-0x1badd00d");
        }
    }
}
