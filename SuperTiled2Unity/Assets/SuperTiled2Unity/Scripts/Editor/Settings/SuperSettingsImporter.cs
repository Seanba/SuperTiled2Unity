using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
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
