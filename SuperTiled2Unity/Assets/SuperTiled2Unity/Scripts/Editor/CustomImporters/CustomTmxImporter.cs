using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SuperTiled2Unity;
using SuperTiled2Unity.Editor;
using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public class TmxAssetImportedArgs
    {
        public TmxAssetImporter AssetImporter { get; set; }
        public SuperMap ImportedSuperMap { get; set; }
    };

    public abstract class CustomTmxImporter
    {
        // Invoked when a Tmx asset import is completed (the prefab and all other objects associated with the asset have been constructed)
        public abstract void TmxAssetImported(TmxAssetImportedArgs args);
    }
}

// Test usage of a custom importer
/*
namespace MyNamespace
{
    public class MyTmxImporter : CustomTmxImporter
    {
        public override void TmxAssetImported(TmxAssetImportedArgs args)
        {
            // Just log the name of the map
            var map = args.ImportedSuperMap;
            Debug.LogFormat("Map '{0}' has been imported.", map.name);
        }
    }

    // Use DisplayNameAttribute to control how class appears in the list
    [DisplayName("My Other Importer")]
    public class MyOtherTmxImporter : CustomTmxImporter
    {
        public override void TmxAssetImported(TmxAssetImportedArgs args)
        {
            // Just log the number of layers in our tiled map
            var map = args.ImportedSuperMap;
            var layers = map.GetComponentsInChildren<SuperLayer>();
            Debug.LogFormat("Map '{0}' has {1} layers.", map.name, layers.Length);
        }
    }
}
*/
