using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TilesetAtlasImporter))]
    public class TilesetAtlasImporterEditor : ScriptedImporterEditor
    {
        public override void OnInspectorGUI()
        {
            // fixit - show an error if our Unity version does not support Sprite Atlas Packing V2 properly (before 2021.3) if we are using a V2 sprite atlas
            if (target is TilesetAtlasImporter importer)
            {
                if (importer.IsUnityVersionIncompatible())
                {
                    // We need 2021.3 or newer in order to use Sprite Atlas V2
                    EditorGUILayout.LabelField("INCOMPATIBLE UNITY VERSION", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox("Tileset Atlas requires Unity 2021.3 or later when using Sprite Atlas V2.\n" +
                        "Potential fixes:\n" +
                        " - Upgrade to Unity 2021.3 or later\n" +
                        " - Select a V1 Sprite Atlas\n" +
                        " - Set 'Project Settings -> Editor -> Sprite Packer -> Mode' to 'Sprite Atlas V1'",
                        MessageType.Error);
                    EditorGUILayout.Space();
                }
            }

            base.OnInspectorGUI();
        }
    }
}
