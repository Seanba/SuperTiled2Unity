using System;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// All tiled assets we want imported should use this class
namespace SuperTiled2Unity.Editor
{
    public abstract class TiledAssetImporter : SuperImporter
    {
        public const string PixelsPerUnitSerializedName = nameof(m_PixelsPerUnit);
        public const string EdgesPerEllipseSerializedName = nameof(m_EdgesPerEllipse);

        [SerializeField]
        private float m_PixelsPerUnit = 0.0f;
        public float PixelsPerUnit => m_PixelsPerUnit;

        public float InversePPU => 1.0f / PixelsPerUnit;

        [SerializeField]
        private int m_EdgesPerEllipse = 0;
        public int EdgesPerEllipse => m_EdgesPerEllipse;

#pragma warning disable 414
        [SerializeField]
        private int m_NumberOfObjectsImported = 0;
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
            var matchedMaterial = ST2USettings.instance.m_MaterialMatchings.FirstOrDefault(m => m.m_LayerName.Equals(match, StringComparison.OrdinalIgnoreCase));
            if (matchedMaterial != null)
            {
                renderer.material = matchedMaterial.m_Material;
                return;
            }

            // Has the user chosen to override the material used for our tilemaps and sprite objects?
            if (ST2USettings.instance.m_DefaultMaterial != null)
            {
                renderer.material = ST2USettings.instance.m_DefaultMaterial;
            }
        }

        public void ApplyTemplateToObject(XElement xObject)
        {
            var template = xObject.GetAttributeAs("template", "");
            if (!string.IsNullOrEmpty(template))
            {
                var asset = RequestDependencyAssetAtPath<ObjectTemplate>(template);
                if (asset != null)
                {
                    xObject.CombineWithTemplate(asset.m_ObjectXml);
                }
            }
        }

        public void ApplyDefaultSettings()
        {
            m_PixelsPerUnit = ST2USettings.instance.m_DefaultPixelsPerUnit;
            m_EdgesPerEllipse = ST2USettings.instance.m_DefaultEdgesPerEllipse;
            EditorUtility.SetDirty(this);
        }

        protected override void InternalOnImportAsset()
        {
            if (m_PixelsPerUnit == 0)
            {
                m_PixelsPerUnit = ST2USettings.instance.m_DefaultPixelsPerUnit;
            }

            if (m_EdgesPerEllipse == 0)
            {
                m_EdgesPerEllipse = ST2USettings.instance.m_DefaultEdgesPerEllipse;
            }

            RendererSorter = new RendererSorter();
            SuperImportContext = new SuperImportContext(AssetImportContext, m_PixelsPerUnit, m_EdgesPerEllipse);
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
                if (CheckTagName(tag))
                {
                    properties.gameObject.tag = tag;
                }
            }
        }

        protected void AssignUnityLayer(SuperCustomProperties properties)
        {
            // Do we have a 'unity:layer' property?
            if (properties.TryGetCustomProperty(StringConstants.Unity_Layer, out CustomProperty prop))
            {
                string layer = prop.m_Value;
                if (CheckLayerName(layer))
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
    }
}
