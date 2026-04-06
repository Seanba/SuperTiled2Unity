namespace SuperTiled2Unity.Editor
{
    public class WorldAssetImportedArgs
    {
        public WorldAssetImporter AssetImporter { get; set; }
        public SuperWorld ImportedSuperWorld { get; set; }
    };

    public abstract class CustomWorldImporter
    {
        // Invoked when a World asset import is completed (the prefab and all other objects associated with the asset have been constructed)
        public abstract void WorldAssetImported(WorldAssetImportedArgs args);
    }
}
