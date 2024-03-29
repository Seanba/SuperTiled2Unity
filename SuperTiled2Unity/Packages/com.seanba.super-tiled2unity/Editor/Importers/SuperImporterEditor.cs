using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public abstract class SuperImporterEditor<T> : ScriptedImporterEditor where T : SuperImporter
    {
        private bool m_ShowDependencies;
        private bool m_ShowReferences;

        public T TargetAssetImporter => serializedObject.targetObject as T;

        protected abstract string EditorLabel { get; }
        protected abstract string EditorDefinition { get; }

        public void ReimportAsset()
        {
#if UNITY_2022_2_OR_NEWER
            SaveChanges();
#else
            ApplyAndImport();
#endif
        }

        public override sealed void OnInspectorGUI()
        {
            if (assetTarget != null)
            {
                // Do we have any import errors to report?
                var importErrors = AssetDatabase.LoadAssetAtPath<ImportErrors>(TargetAssetImporter.assetPath);
                if (importErrors != null)
                {
                    ImportErrorsEditor.ImportErrorsGui(this, importErrors);
                    EditorGUILayout.Separator();
                }

                EditorGUILayout.LabelField(EditorLabel, EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(EditorDefinition, MessageType.None);
                EditorGUILayout.Separator();

                serializedObject.Update();

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
            serializedObject.ApplyModifiedProperties();
            ApplyRevertGUI();
        }

        protected abstract void InternalOnInspectorGUI();

        private void DisplayDependencies()
        {
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("[Dependencies are not diplayed while Editor is in Play Mode]", EditorStyles.boldLabel);
                return;
            }

            if (!TiledAssetDependencies.Instance.GetAssetDependencies(TargetAssetImporter.assetPath, out AssetDependencies depends))
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
    }
}
