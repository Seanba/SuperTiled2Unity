using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    internal class SuperPackageExport : EditorWindow
    {
        internal static class Styles
        {
            public static GUIStyle Title = "LargeBoldLabel";
            public static GUIStyle TopBarBg = "OT TopBar";
            public static GUIContent Header = EditorGUIUtility.TrTextContent("SuperTiled2Unity Dependencies to Export");
        }

        private string m_Name;
        private List<string> m_Assets;
        private Vector2 m_ScrollPosition;

        internal SuperPackageExport()
        {
            // Initial pos and minsize
            position = new Rect(100, 100, 400, 300);
            minSize = new Vector2(350, 350);
        }

        static internal void ShowWindow(string name, IEnumerable<string> assets)
        {
            var window = GetWindow<SuperPackageExport>(true, "Exporting SuperTiled2Unity Asset");
            window.m_Name = name;
            window.m_Assets = new List<string>(assets.OrderBy(a => a));
        }

        public void OnGUI()
        {
            TopArea();
            ListArea();
            BottomArea();
        }

        void TopArea()
        {
            float totalTopHeight = 53f;
            Rect r = GUILayoutUtility.GetRect(position.width, totalTopHeight);

            // Background
            GUI.Label(r, GUIContent.none, Styles.TopBarBg);

            // Header
            Rect titleRect = new Rect(r.x + 5f, r.yMin, r.width, r.height);
            GUI.Label(titleRect, Styles.Header, Styles.Title);
        }

        void BottomArea()
        {
            using (new GUILayout.VerticalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(EditorGUIUtility.TrTextContent("Export...")))
                {
                    Export();
                    GUIUtility.ExitGUI();
                }
            }
        }

        private void ListArea()
        {
            using (new GUILayout.VerticalScope())
            using (var scroll = new GUILayout.ScrollViewScope(m_ScrollPosition))
            {
                m_ScrollPosition = scroll.scrollPosition;

                foreach (var asset in m_Assets)
                {
                    EditorGUILayout.LabelField(asset);
                }
            }
        }

        private void Export()
        {
            string defaultName = string.Format("st2u-export-{0}-files", m_Name);
            string path = EditorUtility.SaveFilePanel("Export package ...", "", defaultName, "unitypackage");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.ExportPackage(m_Assets.ToArray(), path);

                Close();
                GUIUtility.ExitGUI();
            }
        }
    }
}
