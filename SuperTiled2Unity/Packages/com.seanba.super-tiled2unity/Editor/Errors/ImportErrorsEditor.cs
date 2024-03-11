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
                ui.HelpBox($"SuperTiled2Unity version: {SuperTiled2Unity_Config.Version}, Unity version: {Application.unityVersion}\nErrors Detected. Your Tiled asset may not function correctly. Please follow directions to fix.");

                DisplayMissingDependencies(ui, importErrors);
                DisplayWrongPixelsPerUnit(ui, importErrors);
                DisplayDependencyErrors(ui, importErrors);
                DisplayMissingSprites(ui, importErrors);
            }
        }

        private static void DisplayMissingDependencies(MessageBuilderUI ui, ImportErrors importErrors)
        {
            if (importErrors.m_MissingDependencies.Count > 0)
            {
                ui.BoldLabel("Missing Dependencies - Files Are Missing Or Misplaced");

                StringBuilder msg = new StringBuilder(1024 * 4);
                msg.AppendLine("The following assets are needed by Super Tiled2Unity.");
                msg.AppendLine("This asset is dependent on other files that either cannot be found or they failed to be imported.");
                msg.AppendLine("They may be missing entirely or they may be in the wrong folder.");
                msg.AppendLine("Note that all Tiled assets must be imported to Unity in folder locations that keep their relative paths intact.");
                msg.AppendLine();

                foreach (var missing in importErrors.m_MissingDependencies)
                {
                    msg.AppendLine(missing);
                }

                ui.HelpBox(msg.ToString());
            }
        }

        private static void DisplayWrongPixelsPerUnit(MessageBuilderUI ui, ImportErrors importErrors)
        {
            if (importErrors.m_WrongPixelsPerUnits.Count > 0)
            {
                ui.BoldLabel("Mismatched Pixels Per Unit");
                foreach (var wrongPPU in importErrors.m_WrongPixelsPerUnits)
                {
                    var assetName = Path.GetFileName(wrongPPU.m_DependencyAssetPath);
                    ui.HelpBox($"Our Pixels Per Unit setting is '{wrongPPU.m_ExpectingPPU}'\n{assetName} Pixels Per Unit setting is '{wrongPPU.m_DependencyPPU}'\nThese values must match for properly sized tiles.");
                    using (new GuiScopedBackgroundColor(NamedColors.LightPink))
                    {
                        if (GUILayout.Button($"Inspect '{assetName}'"))
                        {
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(wrongPPU.m_DependencyAssetPath);
                        }
                    }
                }
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
                    foreach (var tile in missing.m_MissingSprites.Take(reportCount))
                    {
                        msg.AppendLine($"Missing sprite {tile.m_SpriteId}: Pos = ({tile.m_Rect.x}, {tile.m_Rect.y}), Size = ({tile.m_Rect.width}x{tile.m_Rect.height}), Pivot = (0, 0)");
                    }

                    if (missing.m_MissingSprites.Count > reportCount)
                    {
                        msg.AppendLine($"An additional {missing.m_MissingSprites.Count - reportCount} sprites are missing. Total = {missing.m_MissingSprites.Count}");
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
                            AssetDatabase.ImportAsset(missing.m_TextureAssetPath);
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
                EditorGUILayout.HelpBox($"{msg}\n", MessageType.Error);
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
