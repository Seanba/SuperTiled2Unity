using System;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Tilemaps;

// All tiled assets we want imported should use this class
namespace SuperTiled2Unity.Editor
{
    public abstract class TiledAssetImporter : SuperImporter
    {
        [SerializeField] private float m_PixelsPerUnit = 0.0f;
        public float PixelsPerUnit => m_PixelsPerUnit;

        public float InversePPU => 1.0f / PixelsPerUnit;

        [SerializeField] private int m_EdgesPerEllipse = 0;

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

        public void AssignMaterial(Renderer renderer, string match)
        {
            // Do we have a registered material match?
            var matchedMaterial = ST2USettings.instance.MaterialMatchings.FirstOrDefault(m => m.m_LayerName.Equals(match, StringComparison.OrdinalIgnoreCase));
            if (matchedMaterial != null)
            {
                renderer.material = matchedMaterial.m_Material;
                return;
            }

            // Has the user chosen to override the material used for our tilemaps and sprite objects?
            if (ST2USettings.instance.DefaultMaterial != null)
            {
                renderer.material = ST2USettings.instance.DefaultMaterial;
            }
        }

        public void ApplyTemplateToObject(XElement xObject)
        {
            var template = xObject.GetAttributeAs("template", "");
            if (!string.IsNullOrEmpty(template))
            {
                var asset = RequestAssetAtPath<ObjectTemplate>(template);
                if (asset != null)
                {
                    xObject.CombineWithTemplate(asset.m_ObjectXml);
                }
                else
                {
                    ReportError("Missing template file: {0}", template);
                }
            }
        }

        public void ApplyDefaultSettings()
        {
            m_PixelsPerUnit = ST2USettings.instance.PixelsPerUnit;
            m_EdgesPerEllipse = ST2USettings.instance.EdgesPerEllipse;
            EditorUtility.SetDirty(this);
        }

        protected override void InternalOnImportAsset()
        {
            RendererSorter = new RendererSorter();
            WrapImportContext(AssetImportContext);
        }

        protected override void InternalOnImportAssetCompleted()
        {
            RendererSorter = null;
            m_NumberOfObjectsImported = SuperImportContext.GetNumberOfObjects();

            // Assets should be dirtied upon importing so that their meta files are serialized
            // Without this we may end up with old garbage in our meta files
            EditorUtility.SetDirty(this);
        }

        protected void AssignUnityTag(SuperCustomProperties properties)
        {
            // Do we have a 'unity:tag' property?
            CustomProperty prop;
            if (properties.TryGetCustomProperty(StringConstants.Unity_Tag, out prop))
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
            if (properties.TryGetCustomProperty(StringConstants.Unity_Layer, out prop))
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
            ST2USettings.instance.RefreshCustomObjectTypes();

            // fixit - you must not copy a singleton like this!
            // Create a copy of our settings that we can override based on importer settings
            // investigate furter and temporarily set the values if you need with a using pattern
            // figure out what I'm really trying to do here and fix it
            //var settings = Instantiate(ST2USettings.instance);
            //if (m_PixelsPerUnit == 0)
            //{
            //    m_PixelsPerUnit = settings.PixelsPerUnit;
            //}

            //if (m_EdgesPerEllipse == 0)
            //{
            //    m_EdgesPerEllipse = settings.EdgesPerEllipse;
            //}

            //settings.PixelsPerUnit = m_PixelsPerUnit; // fixit - nope! Don't like it
            //settings.EdgesPerEllipse = m_EdgesPerEllipse;

            SuperImportContext = new SuperImportContext(ctx);
        }
    }
}
