using UnityEditor.Experimental.AssetImporters;

namespace SuperTiled2Unity.Editor
{
    // Note: Our settings is set to be imported first
    [ScriptedImporter(101, "st2u")]
    public class SuperSettingsImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            // Do nothing
        }
    }
}
