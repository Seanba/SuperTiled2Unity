using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

#if UNITY_2020_2_OR_NEWER
using ScriptedImporterEditor = UnityEditor.AssetImporters.ScriptedImporterEditor;
#else
using ScriptedImporterEditor = UnityEditor.Experimental.AssetImporters.ScriptedImporterEditor;
#endif

namespace SuperTiled2Unity.Editor
{
    public abstract class SuperImporterEditor<T> : ScriptedImporterEditor where T : SuperImporter
    {
        private bool m_ShowDependencies;
        private bool m_ShowReferences;

        public T TargetAssetImporter
        {
            get { return serializedObject.targetObject as T; }
        }

        protected abstract string EditorLabel { get; }
        protected abstract string EditorDefinition { get; }

        public override sealed void OnInspectorGUI()
        {
            if (assetTarget != null)
            {
                // If we have importer errors then they should be front and center
                DisplayMissingFileErrors();
                DisplayErrorsAndWarnings();
                DisplayTagManagerErrors();

                EditorGUILayout.LabelField(EditorLabel, EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(EditorDefinition, MessageType.None);
                EditorGUILayout.Separator();

#if UNITY_2019_2_OR_NEWER
                serializedObject.Update();
#endif
                InternalOnInspectorGUI();
                DisplayDependencies();
            }
            else
            {
                ForceDeselectAndExit();
            }
        }

        protected override void OnHeaderGUI()
        {
            if (assetTarget != null)
            {
                base.OnHeaderGUI();
            }
            else
            {
                ForceDeselectAndExit();
            }
        }

        protected U GetAssetTarget<U>() where U : UnityEngine.Object
        {
            foreach (var target in assetTargets)
            {
                U u = target as U;
                if (u != null)
                {
                    return u;
                }
            }

            return null;
        }


        protected void ToggleFromInt(SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            int intValue = EditorGUILayout.Toggle(label, property.intValue > 0) ? 1 : 0;
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = intValue;
            }
        }

        protected void InternalApplyRevertGUI()
        {
#if UNITY_2019_2_OR_NEWER
            serializedObject.ApplyModifiedProperties();
#endif
            ApplyRevertGUI();
        }

        protected abstract void InternalOnInspectorGUI();

        private void DisplayMissingFileErrors()
        {
            using (new GuiScopedBackgroundColor(Color.magenta))
            {
                if (TargetAssetImporter.MissingFiles.Any())
                {
                    var asset = Path.GetFileName(TargetAssetImporter.assetPath);
                    EditorGUILayout.LabelField("Missing or misplaced assets!", EditorStyles.boldLabel);

                    var msg = new StringBuilder();

                    msg.AppendLine(TargetAssetImporter.GetReportHeader());
                    msg.AppendLine("This asset is dependent on other files that either cannot be found or they failed to be imported.");
                    msg.AppendLine("Note that all Tiled assets must be imported to Unity in folder locations that keep their relative paths intact.");
                    msg.AppendLine("Reimport this asset once fixes are made.\n");
                    msg.AppendFormat("Tip: Try opening {0} in Tiled to resolve location of missing assets.\n\n", asset);

                    msg.AppendLine(string.Join("\n", TargetAssetImporter.MissingFiles.ToArray()));

                    EditorGUILayout.HelpBox(msg.ToString(), MessageType.Error);

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Copy Message to Clipboard"))
                        {
                            msg.ToString().CopyToClipboard();
                        }

                        if (GUILayout.Button("Reimport"))
                        {
                            ApplyAndImport();
                        }
                    }

                    EditorGUILayout.Separator();
                }
            }
        }

        private void DisplayErrorsAndWarnings()
        {
            var asset = Path.GetFileName(TargetAssetImporter.assetPath);

            using (new GuiScopedBackgroundColor(Color.red))
            {
                if (TargetAssetImporter.Errors.Any())
                {
                    EditorGUILayout.LabelField("There were errors importing " + asset, EditorStyles.boldLabel);

                    var msg = new StringBuilder();
                    msg.AppendLine(TargetAssetImporter.GetReportHeader());
                    msg.AppendLine(string.Join("\n", TargetAssetImporter.Errors.Take(10).ToArray()));

                    EditorGUILayout.HelpBox(msg.ToString(), MessageType.Error);

                    if (GUILayout.Button("Copy Error Message to Clipboard"))
                    {
                        msg.ToString().CopyToClipboard();
                    }

                    EditorGUILayout.Separator();
                }
            }

            using (new GuiScopedBackgroundColor(Color.yellow))
            {
                if (TargetAssetImporter.Warnings.Any())
                {
                    EditorGUILayout.LabelField("There were warnings importing " + asset, EditorStyles.boldLabel);
                    var msg = string.Join("\n\n", TargetAssetImporter.Warnings.Take(10).ToArray());
                    EditorGUILayout.HelpBox(msg, MessageType.Warning);

                    if (GUILayout.Button("Copy Warning Message to Clipboard"))
                    {
                        msg.ToString().CopyToClipboard();
                    }

                    EditorGUILayout.Separator();
                }
            }
        }

        private void DisplayTagManagerErrors()
        {
            bool missingSortingLayers = TargetAssetImporter.MissingSortingLayers.Any();
            bool missingLayers = TargetAssetImporter.MissingLayers.Any();
            bool missingTags = TargetAssetImporter.MissingTags.Any();

            if (!missingSortingLayers && !missingLayers && !missingTags)
            {
                return;
            }

            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

            using (new GuiScopedBackgroundColor(Color.yellow))
            {
                if (missingSortingLayers)
                {
                    EditorGUILayout.LabelField("Missing Sorting Layers!", EditorStyles.boldLabel);

                    using (new GuiScopedIndent())
                    {
                        StringBuilder message = new StringBuilder("Sorting Layers are missing in your project settings. Open the Tag Manager, add these missing sorting layers, and reimport:");
                        message.AppendLine();
                        message.AppendLine();

                        foreach (var layer in TargetAssetImporter.MissingSortingLayers)
                        {
                            message.AppendFormat("    {0}\n", layer);
                        }

                        EditorGUILayout.HelpBox(message.ToString(), MessageType.Warning);
                    }
                }

                if (missingLayers)
                {
                    EditorGUILayout.LabelField("Missing Layers!", EditorStyles.boldLabel);

                    using (new GuiScopedIndent())
                    {
                        StringBuilder message = new StringBuilder("Layers are missing in your project settings. Open the Tag Manager, add these missing layers, and reimport:");
                        message.AppendLine();
                        message.AppendLine();

                        foreach (var layer in TargetAssetImporter.MissingLayers)
                        {
                            message.AppendFormat("    {0}\n", layer);
                        }

                        EditorGUILayout.HelpBox(message.ToString(), MessageType.Warning);
                    }
                }

                if (missingTags)
                {
                    EditorGUILayout.LabelField("Missing Tags!", EditorStyles.boldLabel);

                    using (new GuiScopedIndent())
                    {
                        StringBuilder message = new StringBuilder("Tags are missing in your project settings. Open the Tag Manager, add these missing tags, and reimport:");
                        message.AppendLine();
                        message.AppendLine();

                        foreach (var tag in TargetAssetImporter.MissingTags)
                        {
                            message.AppendFormat("    {0}\n", tag);
                        }

                        EditorGUILayout.HelpBox(message.ToString(), MessageType.Warning);
                    }
                }
            }

            using (new GUILayout.HorizontalScope())
            {
#if UNITY_2018_3_OR_NEWER
                if (GUILayout.Button("Open Tag Manager"))
                {
                    SettingsService.OpenProjectSettings("Project/Tags and Layers");
                }
#endif
                if (GUILayout.Button("Reimport"))
                {
                    ApplyAndImport();
                }
            }

            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        }

        private void DisplayDependencies()
        {
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("[Dependencies are not diplayed while Editor is in Play Mode]", EditorStyles.boldLabel);
                return;
            }

            AssetDependencies depends;
            if (!TiledAssetDependencies.Instance.GetAssetDependencies(TargetAssetImporter.assetPath, out depends))
            {
                return;
            }

            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Additional Tiled Asset Information", EditorStyles.boldLabel);

            using (new GuiScopedIndent())
            {
                DisplayObjectCount();

                using (new GuiScopedIndent())
                {
                    var title = string.Format("Dependencies ({0})", depends.Dependencies.Count());
                    var tip = "This asset will be automatically reimported when any of these other assets are updated.";
                    var content = new GUIContent(title, tip);
                    m_ShowDependencies = EditorGUILayout.Foldout(m_ShowDependencies, content);

                    if (m_ShowDependencies)
                    {
                        foreach (var asset in depends.Dependencies)
                        {
                            EditorGUILayout.LabelField(asset);

                            // Context menu items for dependencies
                            var clickArea = GUILayoutUtility.GetLastRect();
                            var current = Event.current;
                            if (clickArea.Contains(current.mousePosition) && current.type == EventType.ContextClick)
                            {
                                var assetName = Path.GetFileName(asset);
                                var selectText = string.Format("Select '{0}'", assetName);
                                var reimportText = string.Format("Reimport '{0}'", assetName);

                                var menu = new GenericMenu();
                                menu.AddItem(new GUIContent(selectText), false, MenuCallbackSelect, asset);
                                menu.AddItem(new GUIContent(reimportText), false, MenuCallbackReimport, asset);
                                menu.ShowAsContext();
                                current.Use();
                            }
                        }
                    }
                }

                using (new GuiScopedIndent())
                {
                    var title = string.Format("References ({0})", depends.References.Count());
                    var tip = "This asset is used by this list of other assets.";
                    var content = new GUIContent(title, tip);
                    m_ShowReferences = EditorGUILayout.Foldout(m_ShowReferences, content);

                    if (m_ShowReferences)
                    {
                        foreach (var asset in depends.References)
                        {
                            EditorGUILayout.LabelField(asset);

                            // Context menu items for dependencies
                            var clickArea = GUILayoutUtility.GetLastRect();
                            var current = Event.current;
                            if (clickArea.Contains(current.mousePosition) && current.type == EventType.ContextClick)
                            {
                                var assetName = Path.GetFileName(asset);
                                var selectText = string.Format("Select '{0}'", assetName);

                                var menu = new GenericMenu();
                                menu.AddItem(new GUIContent(selectText), false, MenuCallbackSelect, asset);
                                menu.ShowAsContext();
                                current.Use();
                            }
                        }
                    }
                }
            }
        }

        private void DisplayObjectCount()
        {
            var numberOfObjectsProperty = serializedObject.FindProperty("m_NumberOfObjectsImported");
            if (numberOfObjectsProperty != null)
            {
                var title = string.Format("Object Count: {0}", numberOfObjectsProperty.intValue);
                var tip = "The number of objects imported into this asset.";
                var content = new GUIContent(title, tip);
                EditorGUILayout.LabelField(content, EditorStyles.label);
            }
        }

        private void MenuCallbackSelect(object asset)
        {
            string assetPath = asset.ToString();
            var assetObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
            Selection.activeObject = assetObject;
            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(assetObject);
        }

        private void MenuCallbackReimport(object asset)
        {
            string assetPath = asset.ToString();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        private void ForceDeselectAndExit()
        {
            // Force Unity to null out select and stop OnGUI calls for this editor
            // This is unfortunate but necessary under re-import edge conditions
            Selection.objects = new UnityEngine.Object[0];
            GUIUtility.ExitGUI();
        }

        // Conitional compiles
#if UNITY_2018_1_OR_NEWER
#else
        protected UnityEngine.Object assetTarget
        {
            get
            {
                if (assetEditor != null)
                {
                    return assetEditor.target;
                }
                return null;
            }
        }

        protected UnityEngine.Object[] assetTargets
        {
            get
            {
                if (assetEditor != null)
                {
                    return assetEditor.targets;
                }
                return null;
            }
        }
#endif
    }
}
