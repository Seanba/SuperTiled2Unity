namespace SuperTiled2Unity.Editor
{
    public class TsxAssetImportedArgs
    {
        public TsxAssetImporter AssetImporter { get; set; }
        public SuperTileset ImportedTileset { get; set; }
    };

    public abstract class CustomTsxImporter
    {
        // Invoked when a Tsx asset import is completed (the prefab and all other objects associated with the asset have been constructed)
        public abstract void TsxAssetImported(TsxAssetImportedArgs args);
    }
}
