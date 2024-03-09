using System;
using System.IO;
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

                DisplayDependencyErrors(ui, importErrors);
                DisplayMissingSprites(ui, importErrors);
            }
        }

        private static void DisplayDependencyErrors(MessageBuilderUI ui, ImportErrors importErrors)
        {
            if (importErrors.m_ErrorsInAssetDependencies.Count > 0)
            {
                ui.BoldLabel("Dependency Errors - Inspect Assets For Further Details");
                foreach (var assetPath in importErrors.m_ErrorsInAssetDependencies)
                {
                    ui.HelpBox($"Errors found in file '{assetPath}'\nInspect asset for details.");

                    using (new GuiScopedBackgroundColor(NamedColors.LightPink))
                    {
                        var assetName = Path.GetFileName(assetPath);
                        if (GUILayout.Button($"Inspect '{assetName}'"))
                        {
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
                        }
                    }
                }
            }
        }

        private static void DisplayMissingSprites(MessageBuilderUI ui, ImportErrors importErrors)
        {
            if (importErrors.m_MissingTileSprites.Count > 0)
            {
                ui.BoldLabel("Missing Sprites - Reimporting Textures May Fix");
                foreach (var missing in importErrors.m_MissingTileSprites)
                {
                    StringBuilder msg = new StringBuilder(1024 * 4);
                    msg.AppendLine("The following texture is missing sprites that are needed for Super Tiled2Unity.");
                    msg.AppendLine(missing.m_TextureAssetPath);

                    int reportCount = 5;
                    foreach (var tile in missing.m_MissingTiles.Take(reportCount))
                    {
                        msg.AppendLine($"Missing tile {tile.m_SpriteId}: ({tile.m_Rect.x}, {tile.m_Rect.y}) ({tile.m_Rect.width}x{tile.m_Rect.height})");
                    }

                    if (missing.m_MissingTiles.Count > reportCount)
                    {
                        msg.AppendLine($"An additional {missing.m_MissingTiles.Count - reportCount} sprites are missing. Total = {missing.m_MissingTiles.Count}");
                    }

                    msg.AppendLine("Super Tiled2Unity will attempt to automatically add these missing sprites on import. Try reimporting.");

                    ui.HelpBox(msg.ToString());

                    using (new GuiScopedBackgroundColor(NamedColors.LightPink))
                    {
                        var assetName = Path.GetFileName(missing.m_TextureAssetPath);
                        if (GUILayout.Button($"Inspect '{assetName}'"))
                        {
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(missing.m_TextureAssetPath);
                        }

                        if (GUILayout.Button($"Reimport '{assetName}'"))
                        {
                            AssetDatabase.ImportAsset(missing.m_TextureAssetPath); // fixit - SuperTexturePostprocessor not working correctly yet
                        }
                    }
                }
            }
        }

        private class MessageBuilderUI : IDisposable
        {
            private GuiScopedBackgroundColor m_Background = new GuiScopedBackgroundColor(Color.red);
            private StringBuilder m_StringBuilder = new StringBuilder(1024 * 8);

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
