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
        public static void ImportErrorsGui<T>(SuperImporterEditor<T> editor, ImportErrors importErrors) where T : SuperImporter
        {
            if (importErrors == null)
            {
                return;
            }

            using (var ui = new MessageBuilderUI())
            {
                ui.HelpBox($"SuperTiled2Unity version: {SuperTiled2Unity_Config.Version}, Unity version: {Application.unityVersion}\nErrors Detected. Your Tiled asset may not function correctly. Please follow directions to fix.");

                DisplayMissingDependencies(ui, importErrors);

                if (DisplayWrongTextureSize(ui, importErrors))
                {
                    // Stop here as it must be fixed. Other errors will have to wait.
                    return;
                }

                if (DisplayWrongPixelsPerUnit(ui, importErrors))
                {
                    // Stop here as it must be fixed. Other errors will have to wait.
                    return;
                }

                DisplayDependencyErrors(ui, importErrors);
                DisplayMissingSprites(ui, importErrors);
                DisplayMissingTags(editor, ui, importErrors);
                DisplayMissingLayers(editor, ui, importErrors);
                DisplayMissingSortingLayers(editor, ui, importErrors);
                DisplayGenericErrors(ui, importErrors);
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

        private static bool DisplayWrongTextureSize(MessageBuilderUI ui, ImportErrors importErrors)
        {
            if (importErrors.m_WrongTextureSizes.Count > 0)
            {
                ui.BoldLabel("Mismatched Texture Sizes");
                foreach (var wrongSize in importErrors.m_WrongTextureSizes)
                {
                    var assetName = Path.GetFileName(wrongSize.m_TextureAssetPath);
                    ui.HelpBox($"Expected texture size: {wrongSize.m_ExpectedWidth}x{wrongSize.m_ExpectedHeight}\nActual texture size: {wrongSize.m_ActualWidth}x{wrongSize.m_ActualHeight}\nCheck import settings for '{assetName}'.\nMax Size may be too small.");
                    using (new GuiScopedBackgroundColor(NamedColors.LightPink))
                    {
                        if (GUILayout.Button($"Inspect '{assetName}'"))
                        {
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(wrongSize.m_TextureAssetPath);
                        }
                    }
                }

                return true;
            }

            return false;
        }

        private static bool DisplayWrongPixelsPerUnit(MessageBuilderUI ui, ImportErrors importErrors)
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

                return true;
            }

            return false;
        }

        private static void DisplayDependencyErrors(MessageBuilderUI ui, ImportErrors importErrors)
        {
            if (importErrors.m_ErrorsInAssetDependencies.Count > 0)
            {
                ui.BoldLabel("Dependency Errors - Inspect Assets For Further Details");
                foreach (var dependencyErrors in importErrors.m_ErrorsInAssetDependencies)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"Errors found in file '{dependencyErrors.m_DependencyAssetPath}'");

                    foreach (var reason in dependencyErrors.m_Reasons)
                    {
                        builder.AppendLine(reason);
                    }

                    builder.AppendLine("Inspect asset for details.");
                    ui.HelpBox(builder.ToString());

                    using (new GuiScopedBackgroundColor(NamedColors.LightPink))
                    {
                        var assetName = Path.GetFileName(dependencyErrors.m_DependencyAssetPath);
                        if (GUILayout.Button($"Inspect '{assetName}'"))
                        {
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(dependencyErrors.m_DependencyAssetPath);
                        }
                    }
                }
            }
        }

        private static void DisplayMissingSprites(MessageBuilderUI ui, ImportErrors importErrors)
        {
            if (importErrors.m_MissingTileSprites.Count > 0)
            {
                ui.BoldLabel("Missing Sprites");
                foreach (var missing in importErrors.m_MissingTileSprites)
                {
                    var assetName = Path.GetFileName(missing.m_TextureAssetPath);
                    StringBuilder msg = new StringBuilder(1024 * 4);
                    msg.AppendLine("The following texture is missing sprites that are needed for Tiled tilesets (in Super Tiled2Unity).");
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

                    ui.HelpBox(msg.ToString());

                    using (new GuiScopedBackgroundColor(NamedColors.LightPink))
                    {
                        if (GUILayout.Button($"Inspect '{assetName}'"))
                        {
                            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(missing.m_TextureAssetPath);
                        }
                    }
                }
            }
        }

        private static void DisplayMissingTags<T>(SuperImporterEditor<T> editor, MessageBuilderUI ui, ImportErrors importErrors) where T : SuperImporter
        {
            if (importErrors.m_MissingTags.Count > 0)
            {
                ui.BoldLabel("Missing Tags - Fix in Tag Manager");

                StringBuilder msg = new StringBuilder(1024 * 4);
                msg.AppendLine("The following tags are missing in the Tag Manager.");
                msg.AppendLine("Open the Tag Manager, add the missing tags, and reimport.");

                foreach (var tag in importErrors.m_MissingTags)
                {
                    msg.AppendLine(tag);
                }

                ui.HelpBox(msg.ToString());

                using (new GuiScopedBackgroundColor(NamedColors.LightPink))
                {
                    if (GUILayout.Button("Open Tag Manager"))
                    {
                        SettingsService.OpenProjectSettings("Project/Tags and Layers");
                    }

                    if (GUILayout.Button($"Reimport"))
                    {
                        editor.ReimportAsset();
                    }
                }
            }
        }

        private static void DisplayMissingLayers<T>(SuperImporterEditor<T> editor, MessageBuilderUI ui, ImportErrors importErrors) where T : SuperImporter
        {
            if (importErrors.m_MissingLayers.Count > 0)
            {
                ui.BoldLabel("Missing Layers - Fix in Tag Manager");

                StringBuilder msg = new StringBuilder(1024 * 4);
                msg.AppendLine("The following layers are missing in the Tag Manager.");
                msg.AppendLine("Open the Tag Manager, add the missing layers, and reimport.");

                foreach (var layer in importErrors.m_MissingLayers)
                {
                    msg.AppendLine(layer);
                }

                ui.HelpBox(msg.ToString());

                using (new GuiScopedBackgroundColor(NamedColors.LightPink))
                {
                    if (GUILayout.Button("Open Tag Manager"))
                    {
                        SettingsService.OpenProjectSettings("Project/Tags and Layers");
                    }

                    if (GUILayout.Button($"Reimport"))
                    {
                        editor.ReimportAsset();
                    }
                }
            }
        }

        private static void DisplayMissingSortingLayers<T>(SuperImporterEditor<T> editor, MessageBuilderUI ui, ImportErrors importErrors) where T : SuperImporter
        {
            if (importErrors.m_MissingSortingLayers.Count > 0)
            {
                ui.BoldLabel("Missing Sorting Layers - Fix in Tag Manager");

                StringBuilder msg = new StringBuilder(1024 * 4);
                msg.AppendLine("The following sorting layers are missing in the Tag Manager.");
                msg.AppendLine("Open the Tag Manager, add the missing sorting layers, and reimport.");

                foreach (var sortingLayer in importErrors.m_MissingSortingLayers)
                {
                    msg.AppendLine(sortingLayer);
                }

                ui.HelpBox(msg.ToString());

                using (new GuiScopedBackgroundColor(NamedColors.LightPink))
                {
                    if (GUILayout.Button("Open Tag Manager"))
                    {
                        SettingsService.OpenProjectSettings("Project/Tags and Layers");
                    }

                    if (GUILayout.Button($"Reimport"))
                    {
                        editor.ReimportAsset();
                    }
                }
            }
        }

        private static void DisplayGenericErrors(MessageBuilderUI ui, ImportErrors importErrors)
        {
            if (importErrors.m_GenericErrors.Count > 0)
            {
                ui.BoldLabel("Generic Errors");

                StringBuilder msg = new StringBuilder(1024 * 8);
                foreach (var error in importErrors.m_GenericErrors)
                {
                    msg.AppendLine(error);
                }

                ui.HelpBox(msg.ToString());
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
