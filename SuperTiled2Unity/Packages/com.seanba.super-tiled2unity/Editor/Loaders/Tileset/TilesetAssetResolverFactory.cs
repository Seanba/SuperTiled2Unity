using System.IO;
using UnityEditor;
using UnityEditor.U2D.Aseprite;

namespace SuperTiled2Unity.Editor
{
    internal static class TilesetAssetResolverFactory
    {
        internal static TilesetAssetResolver CreateFromRelativeAssetPath(TiledAssetImporter tiledAssetImporter, SuperTileset superTileset, string relativeAssetPath)
        {
            // The relative path passed in is relative to the asset path of the Tiled Asset
            // Asset is not in our cache so load it from the asset database
            string absPath;
            if (Path.IsPathRooted(relativeAssetPath))
            {
                // Rooted (and absolute) paths baked into Tiled files are not recommended but if they resolve to a Unity asset then so be it.
                absPath = Path.GetFullPath(relativeAssetPath);
            }
            else
            {
                // Passed-in path should be relative to the tiled asset being imported
                var tiledAssetFolder = Path.GetDirectoryName(tiledAssetImporter.assetPath);
                var combinedPath = Path.Combine(tiledAssetFolder, relativeAssetPath);
                absPath = Path.GetFullPath(combinedPath);
            }

            // Turn the absolute path of the dependency into an assetPath if possible
            // This will fail if the absPath is not within this project
            string requestedAssetPath = absPath;
            if (!AssetPath.TryAbsoluteToAsset(ref requestedAssetPath))
            {
                tiledAssetImporter.ReportMissingDependency(absPath);
                return new TilesetAssetResolverError(requestedAssetPath, tiledAssetImporter, superTileset);
            }

            // Keep track that the asset is a dependency
            // We do this even if the asset doesn't exist (as long as we have a valid asset path)
            // The dependency asset may not yet be imported
            // Having a dependency on the import artifact will force the tiled asset to be re-imported once the dependency import completes
            tiledAssetImporter.AssetImportContext.DependsOnArtifact(requestedAssetPath);

            // Get the importer for the dependeny asset
            // This will tell us which kind of resolver to create
            var dependencyImporter = AssetImporter.GetAtPath(requestedAssetPath);

            if (dependencyImporter is TextureImporter)
            {
                return new TilesetAssetResolverTexture(requestedAssetPath, tiledAssetImporter, superTileset);
            }
            else if (dependencyImporter is AsepriteImporter asepriteImporter)
            {
                return new TilesetAssetResolverAseprite(requestedAssetPath, tiledAssetImporter, superTileset, asepriteImporter);
            }

            tiledAssetImporter.ReportGenericError($"Unknown asset importer for '{requestedAssetPath}'");
            return new TilesetAssetResolverError(requestedAssetPath, tiledAssetImporter, superTileset);
        }
    }
}
