namespace SuperTiled2Unity.Editor
{
    public static class ImporterConstants
    {
        public const int TilesetVersion = 20;
        public const int TemplateVersion = 6;
        public const int MapVersion = 31;
        public const int WorldVersion = 1;

        public const string TilesetExtension = "tsx";
        public const string TemplateExtension = "tx";
        public const string MapExtension = "tmx";
        public const string WorldExtension = "world";

        // The order we import Tiled assets is important due to dependencies
        public const int TilesetImportOrder = 5010;
        public const int TemplateImportOrder = 5011;
        public const int MapImportOrder = 5012;
        public const int WorldImportOrder = 5013;
    }
}
