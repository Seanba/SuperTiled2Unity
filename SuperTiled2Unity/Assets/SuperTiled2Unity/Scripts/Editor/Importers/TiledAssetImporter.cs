using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

// All tiled assets we want imported should use this class
namespace SuperTiled2Unity.Editor
{
    public abstract class TiledAssetImporter : SuperImporter
    {
        [SerializeField] private float m_PixelsPerUnit = 0.0f;
        [SerializeField] private int m_EdgesPerEllipse = 0;
        [SerializeField] private SpriteAtlas m_SpriteAtlas = null;

#pragma warning disable 414
        [SerializeField] private int m_NumberOfObjectsImported = 0;
#pragma warning restore 414

        public RendererSorter RendererSorter { get; private set; }

        public SuperImportContext SuperImportContext { get; private set; }

        public void AddSuperCustomProperties(GameObject go, XElement xProperties)
        {
            AddSuperCustomProperties(go, xProperties, null);
        }

        public void AddSuperCustomProperties(GameObject go, XElement xProperties, string typeName)
        {
            AddSuperCustomProperties(go, xProperties, null, typeName);
        }

        public void AddSuperCustomProperties(GameObject go, XElement xProperties, SuperTile tile, string typeName)
        {
            // Load our "local" properties first
            var component = go.AddComponent<SuperCustomProperties>();
            var properties = CustomPropertyLoader.LoadCustomPropertyList(xProperties);

            // Do we have any properties from a tile to add?
            if (tile != null)
            {
                properties.CombineFromSource(tile.m_CustomProperties);
            }

            // Add properties from our object type (this should be last)
            properties.AddPropertiesFromType(typeName, SuperImportContext);

            // Sort the properties alphabetically
            component.m_Properties = properties.OrderBy(p => p.m_Name).ToList();

            AssignUnityTag(component);
            AssignUnityLayer(component);
        }

        public void AssignTilemapSorting(TilemapRenderer renderer)
        {
            var sortLayerName = RendererSorter.AssignTilemapSort(renderer);
            CheckSortingLayerName(sortLayerName);
        }

        public void AssignSpriteSorting(SpriteRenderer renderer)
        {
            var sortLayerName = RendererSorter.AssignSpriteSort(renderer);
            CheckSortingLayerName(sortLayerName);
        }

        public void AssignMaterial(Renderer renderer)
        {
            // Has the user chosen to override the material used for our tilemaps and sprite objects?
            if (SuperImportContext.Settings.DefaultMaterial != null)
            {
                renderer.material = SuperImportContext.Settings.DefaultMaterial;
            }
        }

        protected override void InternalOnImportAsset()
        {
            // Remove (old) sprites from (old) sprite atlas
            //SpriteAtlasUserAsset.RemoveSpritesFromAtlas(assetPath);

            RendererSorter = new RendererSorter();
            WrapImportContext(AssetImportContext);
        }

        protected override void InternalOnImportAssetCompleted()
        {
            // fixit - are we adding a sprite atlas user for the first time? If so it should start off with the Default sprite atlas.
            var atlasUser = SpriteAtlasUserAsset.GetAsset(assetPath);
            if (atlasUser == null)
            {
                m_SpriteAtlas = SpriteAtlasUserAsset.FindDefaultSpriteAtlas();
            }
            else
            {
                SpriteAtlasUserAsset.RemoveSpritesFromAtlas(assetPath);
            }

            // Add sprites to sprite atlas (or more correctly, add the scritable object that will add the sprites when import completes)
            var spriteAtlasUser = SpriteAtlasUserAsset.CreateSpriteAtlasUserAsset(m_SpriteAtlas);
            SuperImportContext.AddObjectToAsset("__atlas", spriteAtlasUser);

            RendererSorter = null;
            m_NumberOfObjectsImported = SuperImportContext.GetNumberOfObjects();
        }

        protected void AssignUnityTag(SuperCustomProperties properties)
        {
            // Do we have a 'unity:tag' property?
            CustomProperty prop;
            if (properties.TryGetCustomProperty("unity:tag", out prop))
            {
                string tag = prop.m_Value;
                CheckTagName(tag);
                properties.gameObject.tag = tag;
            }
        }

        protected void AssignUnityLayer(SuperCustomProperties properties)
        {
            // Do we have a 'unity:layer' property?
            CustomProperty prop;
            if (properties.TryGetCustomProperty("unity:layer", out prop))
            {
                string layer = prop.m_Value;
                if (!UnityEditorInternal.InternalEditorUtility.layers.Contains(layer))
                {
                    string report = string.Format("Layer '{0}' is not defined in the Tags and Layers settings.", layer);
                    ReportError(report);
                }
                else
                {
                    properties.gameObject.layer = LayerMask.NameToLayer(layer);
                }
            }
            else
            {
                // Inherit the layer of our parent
                var parent = properties.gameObject.transform.parent;
                if (parent != null)
                {
                    properties.gameObject.layer = parent.gameObject.layer;
                }
            }
        }

        private void WrapImportContext(AssetImportContext ctx)
        {
            var settings = ST2USettings.GetOrCreateST2USettings();
            settings.RefreshCustomObjectTypes();

            // Create a copy of our settings that we can override based on importer settings
            settings = Instantiate(settings);
            settings.PixelsPerUnit = m_PixelsPerUnit;
            settings.EdgesPerEllipse = m_EdgesPerEllipse;

            SuperImportContext = new SuperImportContext(ctx, settings);
        }
    }
}
