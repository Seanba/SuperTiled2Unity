using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class ImportErrorsEditor
    {
        public static void ImportErrorsGui(ImportErrors importErrors)
        {
            if (importErrors == null)
            {
                return;
            }

            using (var ui = new MessageBuilderUI())
            {
                ui.HelpBox("Errors Detected. Your Tiled asset may not look or function correctly. Please follow directions to fix.");

                DisplayDependencyErrors(importErrors); // fixit -come back and finish this
                DisplayMissingSprites(ui, importErrors);
            }
        }

        private static void DisplayDependencyErrors(ImportErrors importErrors)
        {
            if (importErrors.m_ErrorsInAssetDependencies.Count > 0)
            {
                EditorGUILayout.LabelField("Errors in asset dependencies", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("fixit - dependency errors!", MessageType.Error);
            }
        }

        private static void DisplayMissingSprites(MessageBuilderUI ui, ImportErrors importErrors)
        {
            if (importErrors.m_MissingTileSprites.Count > 0)
            {
                // fixit - need opportunity to select and reimport the texture files in question
                ui.BoldLabel("Missing Sprites - Reimporting Textures May Fix");
                foreach (var missing in importErrors.m_MissingTileSprites)
                {
                    ui.HelpBox("The following texture is missing sprites that are needed for Super Tiled2Unity.", missing.m_TextureAssetPath);
                }
            }
        }

        private class MessageBuilderUI : IDisposable
        {
            private GuiScopedBackgroundColor m_Background = new GuiScopedBackgroundColor(Color.red);
            private StringBuilder m_StringBuilder = new StringBuilder(1024 * 8);

            public void HelpBox(params string[] msgs)
            {
                HelpBox(string.Join("\n", msgs));
            }

            public void HelpBox(string msg)
            {
                m_StringBuilder.AppendLine(msg);
                EditorGUILayout.HelpBox(msg, MessageType.Error);
            }

            public void BoldLabel(string label)
            {
                m_StringBuilder.AppendLine(label);
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            }

            public void Dispose()
            {
                EditorGUILayout.Separator();
                if (GUILayout.Button("Copy Errors to Clipboard"))
                {
                    m_StringBuilder.ToString().CopyToClipboard();
                }

                m_Background.Dispose();
                m_Background = null;
            }
        }
    }
}
