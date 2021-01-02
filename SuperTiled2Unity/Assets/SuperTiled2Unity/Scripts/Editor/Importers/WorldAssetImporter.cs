using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

#if UNITY_2020_2_OR_NEWER
using ScriptedImporterAttribute = UnityEditor.AssetImporters.ScriptedImporterAttribute;
#else
using ScriptedImporterAttribute = UnityEditor.Experimental.AssetImporters.ScriptedImporterAttribute;
#endif

namespace SuperTiled2Unity.Editor
{
    [ScriptedImporter(ImporterConstants.WorldVersion, ImporterConstants.WorldExtension, ImporterConstants.WorldImportOrder)]
    public class WorldAssetImporter : TiledAssetImporter
    {
        private enum WorldType
        {
            world,
        }

        [Serializable]
        private class JsonMap
        {
            #pragma warning disable 0649
            public string fileName;
            public int x;
            public int y;
            #pragma warning restore 0649
        }

        [Serializable]
        private class JsonPattern
        {
            #pragma warning disable 0649
            public string regexp;
            public int multiplierX;
            public int multiplierY;
            public int offsetX;
            public int offsetY;
            #pragma warning restore 0649
        }

        [Serializable]
        private class JsonWorld
        {
            #pragma warning disable 0649
            public List<JsonMap> maps;
            public List<JsonPattern> patterns;
            public WorldType type;
            #pragma warning restore 0649
        }

        protected override void InternalOnImportAsset()
        {
            base.InternalOnImportAsset();
            ImporterVersion = ImporterConstants.WorldVersion;
            AddSuperAsset<SuperAssetWorld>();

            var goWorld = new GameObject("world");

            var icon = SuperIcons.GetWorldIcon();
            SuperImportContext.AddObjectToAsset("_world", goWorld, icon);
            SuperImportContext.SetMainObject(goWorld);

            goWorld.AddComponent<SuperWorld>();

            try
            {
                ParseJsonAsset(goWorld);
            }
            catch (Exception ex)
            {
                ReportError("Unknown error importing World file: {0}\n{1}\n{2}", assetPath, ex.Message, ex.StackTrace);
            }
        }

        private void ParseJsonAsset(GameObject goWorld)
        {
            var json = File.ReadAllText(assetPath);
            JsonWorld jsonWorld = null;

            try
            {
                jsonWorld = JsonUtility.FromJson<JsonWorld>(json);
            }
            catch (Exception ex)
            {
                ReportError("World file has broken JSON syntax.\n{0}", ex.Message);
                return;
            }

            ParseMaps(goWorld, jsonWorld);
            PrasePatterns(goWorld, jsonWorld);
        }

        private void ParseMaps(GameObject goWorld, JsonWorld jsonWorld)
        {
            using (new ChDir(assetPath))
            {
                foreach (var map in jsonWorld.maps)
                {
                    InstantiateMap(goWorld, map);
                }
            }
        }

        private void InstantiateMap(GameObject goWorld, JsonMap jsonMap)
        {
            var path = jsonMap.fileName;
            var superMap = RequestAssetAtPath<SuperMap>(path);

            if (superMap != null && AssetPath.TryRelativeToAsset(ref path))
            {
                // Use the importer of the map to determine Pixels Per Unit
                var mapImporter = (TmxAssetImporter)AssetImporter.GetAtPath(path);
                float x = mapImporter.InversePPU * jsonMap.x;
                float y = -mapImporter.InversePPU * jsonMap.y;
                var go = (GameObject)PrefabUtility.InstantiatePrefab(superMap.gameObject);
                go.transform.SetParent(goWorld.transform);
                go.transform.localPosition = new Vector3(x, y, 0);
            }
        }

        private void PrasePatterns(GameObject goWorld, JsonWorld jsonWorld)
        {
            using (new ChDir(assetPath))
            {
                foreach (var pattern in jsonWorld.patterns)
                {
                    // Find all files in this directory that match the pattern
                    using (new ChDir(assetPath))
                    {
                        foreach (var f in Directory.GetFiles(".", "*.tmx"))
                        {
                            var matches = Regex.Matches(f, pattern.regexp);
                            if (matches.Count >= 1 && matches[0].Groups.Count >= 3)
                            {
                                var x = matches[0].Groups[1].Value.ToInt();
                                var y = matches[0].Groups[2].Value.ToInt();

                                var map = new JsonMap
                                {
                                    fileName = f,
                                    x = (x * pattern.multiplierX) + pattern.offsetX,
                                    y = (y * pattern.multiplierY) + pattern.offsetY
                                };

                                InstantiateMap(goWorld, map);
                            }
                        }
                    }
                }
            }
        }
    }
}
