using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    // Collection of icons to be used with SuperTiled2Unity assets
    public static class SuperIcons
    {
        private static Texture2D m_SettingsIcon;
        private static Texture2D m_TmxIcon;
        private static Texture2D m_TsxIcon;
        private static Texture2D m_TxIcon;

        public static Texture2D GetSettingsIcon()
        {
            if (m_SettingsIcon == null)
            {
                m_SettingsIcon = AssetDatabaseEx.LoadFirstAssetByFilter<Texture2D>("tiled-settings-icon-0x1badd00d");
            }

            return m_SettingsIcon;
        }

        public static Texture2D GetTmxIcon()
        {
            if (m_TmxIcon == null)
            {
                m_TmxIcon = AssetDatabaseEx.LoadFirstAssetByFilter<Texture2D>("tmx-file-icon-0x1badd00d");
            }

            return m_TmxIcon;
        }

        public static Texture2D GetTsxIcon()
        {
            if (m_TsxIcon == null)
            {
                m_TsxIcon = AssetDatabaseEx.LoadFirstAssetByFilter<Texture2D>("tsx-file-icon-0x1badd00d");
            }

            return m_TsxIcon;
        }

        public static Texture2D GetTxIcon()
        {
            if (m_TxIcon == null)
            {
                m_TxIcon = AssetDatabaseEx.LoadFirstAssetByFilter<Texture2D>("tsx-file-icon-0x1badd00d");
            }

            return m_TxIcon;
        }
    }
}
