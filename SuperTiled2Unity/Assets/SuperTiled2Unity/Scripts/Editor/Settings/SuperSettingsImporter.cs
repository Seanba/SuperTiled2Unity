#if UNITY_2020_2_OR_NEWER
using AssetImportContext = UnityEditor.AssetImporters.AssetImportContext;
using ScriptedImporter = UnityEditor.AssetImporters.ScriptedImporter;
using ScriptedImporterAttribute = UnityEditor.AssetImporters.ScriptedImporterAttribute;
#else
using AssetImportContext = UnityEditor.Experimental.AssetImporters.AssetImportContext;
using ScriptedImporter = UnityEditor.Experimental.AssetImporters.ScriptedImporter;
using ScriptedImporterAttribute = UnityEditor.Experimental.AssetImporters.ScriptedImporterAttribute;
#endif

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
