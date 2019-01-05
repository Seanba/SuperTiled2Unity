using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperTiled2Unity.Editor
{
    public static class ImporterConstants
    {
        public const int SettingsVersion = 3;
        public const int TilesetVersion = 5;
        public const int TemplateVersion = 3;
        public const int MapVersion = 6;

        public const string SettingsExtension = "st2u";
        public const string TilesetExtension = "tsx";
        public const string TemplateExtension = "tx";
        public const string MapExtension = "tmx";

        // The order we import Tiled assets is important due to dependencies
        public const int SettingsImportOrder = 1;
        public const int TilesetImportOrder = 10;
        public const int TemplateImportOrder = 11;
        public const int MapImportOrder = 12;
    }
}
