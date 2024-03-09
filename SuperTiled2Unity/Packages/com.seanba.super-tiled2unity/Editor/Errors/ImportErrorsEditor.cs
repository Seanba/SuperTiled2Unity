using UnityEditor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class ImportErrorsEditor
    {
        public static void ImportErrorsGui(ImportErrors importErrors)
        {
            // fixit:left
            DisplayDependencyErrors(importErrors);
            DisplayMissingSprites(importErrors);
        }

        private static void DisplayDependencyErrors(ImportErrors importErrors)
        {
            if (importErrors.m_ErrorsInAssetDependencies.Count > 0)
            {
                using (new GuiScopedBackgroundColor(Color.magenta))
                {
                    EditorGUILayout.HelpBox("fixit - dependency errors!", MessageType.Error);
                }
            }
        }

        private static void DisplayMissingSprites(ImportErrors importErrors)
        {
            if (importErrors.m_MissingTileSprites.Count > 0)
            {
                using (new GuiScopedBackgroundColor(Color.magenta))
                {
                    EditorGUILayout.HelpBox("fixit - missing tiles!", MessageType.Error);
                }
            }
        }
    }
}
