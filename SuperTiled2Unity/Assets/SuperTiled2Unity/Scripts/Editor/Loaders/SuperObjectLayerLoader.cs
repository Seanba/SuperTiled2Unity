using System.Linq;
using System.Xml.Linq;
using SuperTiled2Unity.Editor.Geometry;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperTiled2Unity.Editor
{
    public class SuperObjectLayerLoader : SuperLayerLoader
    {
        private SuperObjectLayer m_ObjectLayer;
        private float m_AnimationFramerate = 1.0f;

        public SuperObjectLayerLoader(XElement xml)
            : base(xml)
        {
        }

        public TiledAssetImporter Importer { get; set; }
        public ColliderFactory ColliderFactory { get; set; }
        public GlobalTileDatabase GlobalTileDatabase { get; set; }
        public SuperMap SuperMap { get; set; }

        public float AnimationFramerate
        {
            get { return m_AnimationFramerate; }
            set { m_AnimationFramerate = value; }
        }

        public void CreateObjects()
        {
            Assert.IsNotNull(m_Xml);
            Assert.IsNotNull(m_ObjectLayer);
            Assert.IsNotNull(Importer);
            Assert.IsNotNull(ColliderFactory);

            var xObjects = m_Xml.Elements("object");
            var drawOrder = m_Xml.GetAttributeAs<DrawOrder>("draworder", DrawOrder.TopDown);

            if (drawOrder == DrawOrder.TopDown)
            {
                // xObjects need to be ordered by y-value
                xObjects = xObjects.OrderBy(x => x.GetAttributeAs<float>("y", 0.0f));
            }

            foreach (var xObj in xObjects)
            {
                // Ignore invisible objects
                if (!xObj.GetAttributeAs<bool>("visible", true))
                {
                    continue;
                }

                CreateObject(xObj);
            }
        }

        protected override SuperLayer CreateLayerComponent(GameObject go)
        {
            m_ObjectLayer = go.AddComponent<SuperObjectLayer>();
            return m_ObjectLayer;
        }

        protected override void InternalLoadFromXml(GameObject go)
        {
            m_ObjectLayer.m_Color = m_Xml.GetAttributeAsColor("color", Color.grey);
        }

        private void CreateObject(XElement xObject)
        {
            // Templates may add extra data
            Importer.ApplyTemplateToObject(xObject);

            // Create the super object and fill it out
            var superObject = CreateSuperObject(xObject);
            FillSuperObject(superObject, xObject);

            // Take care of properties
            Importer.AddSuperCustomProperties(superObject.gameObject, xObject.Element("properties"), superObject.m_SuperTile, superObject.m_Type);

            // Post processing after custom properties have been set
            PostProcessObject(superObject.gameObject);
        }

        private SuperObject CreateSuperObject(XElement xObject)
        {
            // Create the object
            GameObject goObject = new GameObject();
            var comp = goObject.AddComponent<SuperObject>();

            // Fill out the attributes
            comp.m_Id = xObject.GetAttributeAs("id", 0);
            comp.m_TiledName = xObject.GetAttributeAs("name", string.Format("Object_{0}", comp.m_Id));
            comp.m_Type = xObject.GetAttributeAs("type", "");
            comp.m_X = xObject.GetAttributeAs("x", 0.0f);
            comp.m_Y = xObject.GetAttributeAs("y", 0.0f);
            comp.m_Rotation = xObject.GetAttributeAs("rotation", 0.0f);
            comp.m_Width = xObject.GetAttributeAs("width", 0.0f);
            comp.m_Height = xObject.GetAttributeAs("height", 0.0f);
            comp.m_TileId = xObject.GetAttributeAs<uint>("gid", 0);
            comp.m_Visible = xObject.GetAttributeAs("visible", true);
            comp.m_Template = xObject.GetAttributeAs("template", "");

            // Assign the name of our game object
            if (comp.m_TileId != 0)
            {
                // The tile object name is decorated. A descendent will have the "real" object name.
                goObject.name = string.Format("{0} (TRS)", comp.m_TiledName);
            }
            else
            {
                goObject.name = comp.m_TiledName;
            }

            // Position the game object
            var localPosition = new Vector2(comp.m_X, comp.m_Y);
            localPosition = ColliderFactory.TransformPoint(localPosition);
            localPosition = Importer.SuperImportContext.MakePoint(localPosition);

            goObject.transform.localPosition = localPosition;
            goObject.transform.localRotation = Quaternion.Euler(0, 0, Importer.SuperImportContext.MakeRotation(comp.m_Rotation));

            // Add our object to the parent layer
            m_ObjectLayer.gameObject.AddChildWithUniqueName(goObject);

            return comp;
        }

        private void FillSuperObject(SuperObject superObject, XElement xObject)
        {
            // Determine which type of object we are
            var xPolygon = xObject.Element("polygon");
            var xPolyline = xObject.Element("polyline");
            var xEllipse = xObject.Element("ellipse");
            var xPoint = xObject.Element("point");
            var xText = xObject.Element("text");

            bool collisions = Importer.SuperImportContext.LayerIgnoreMode != LayerIgnoreMode.Collision;

            if (superObject.m_TileId != 0)
            {
                ProcessTileObject(superObject, xObject);
            }
            else if (xPolygon != null && collisions)
            {
                ProcessPolygonElement(superObject.gameObject, xPolygon);
            }
            else if (xPolyline != null && collisions)
            {
                ProcessPolylineElement(superObject.gameObject, xPolyline);
            }
            else if (xEllipse != null && collisions)
            {
                ProcessEllipseElement(superObject.gameObject, xObject);
            }
            else if (xText != null)
            {
                // Text objects are not yet supported
            }
            else if (xPoint != null)
            {
                // A point is simply an empty game object out in space.
                // We don't need to add anything else
            }
            else if (collisions)
            {
                // Default object is a rectangle
                ProcessObjectRectangle(superObject.gameObject, xObject);
            }
        }

        private void ProcessTileObject(SuperObject superObject, XElement xObject)
        {
            Assert.IsNull(superObject.m_SuperTile);
            Assert.IsNotNull(GlobalTileDatabase, "Cannot process tile objects without a tileset database");

            SuperTile tile = null;
            var tileId = new TileIdMath(superObject.m_TileId);
            int justTileId = tileId.JustTileId;

            // Are we getting the tile from a template?
            var template = xObject.GetAttributeAs("template", "");
            if (!string.IsNullOrEmpty(template))
            {
                var asset = Importer.RequestAssetAtPath<ObjectTemplate>(template);
                if (asset == null)
                {
                    Importer.ReportError("Template file '{0}' was not found.", template);
                    return;
                }

                tile = asset.m_Tile;
                if (tile == null)
                {
                    Importer.ReportError("Missing tile '{0}' from template '{1}' on tile object '{2}'", justTileId, template, superObject.name);
                    return;
                }
            }

            // Are we getting the tile from our tile database?
            if (tile == null)
            {
                GlobalTileDatabase.TryGetTile(justTileId, out tile);

                if (tile == null)
                {
                    Importer.ReportError("Missing tile '{0}' on tile object '{1}'", justTileId, template, superObject.name);
                    return;
                }
            }

            // Our type may come from the tile as well (this is 'Typed Tiles' in Tiled)
            if (string.IsNullOrEmpty(superObject.m_Type))
            {
                superObject.m_Type = tile.m_Type;
            }

            // Construct the game objects for displaying a single tile
            var inversePPU = Importer.SuperImportContext.Settings.InversePPU;
            bool flip_h = tileId.HasHorizontalFlip;
            bool flip_v = tileId.HasVerticalFlip;

            var scale = Vector3.one;
            scale.x = xObject.GetAttributeAs("width", 1.0f);
            scale.y = xObject.GetAttributeAs("height", 1.0f);

            scale.x /= tile.m_Width;
            scale.y /= tile.m_Height;

            var tileOffset = new Vector3(tile.m_TileOffsetX * inversePPU, -tile.m_TileOffsetY * inversePPU);
            var pivotOffset = ObjectAlignmentToPivot.ToVector3(tile.m_Width, tile.m_Height, inversePPU, SuperMap.m_Orientation, tile.m_ObjectAlignment);

            var translateCenter = new Vector3(tile.m_Width * 0.5f * inversePPU, tile.m_Height * 0.5f * inversePPU);

            // Our root object will contain the translation, rotation, and scale of the tile object
            var goTRS = superObject.gameObject;
            goTRS.transform.localScale = scale;

            // Our pivot object will contain the tileset orientation and offset
            var goPivot = new GameObject();
            goPivot.name = string.Format("{0} (Pivot)", superObject.m_TiledName);
            goPivot.transform.localPosition = tileOffset + pivotOffset;
            goTRS.AddChildWithUniqueName(goPivot);

            // Add another object to handle tile flipping
            // This object will center us into the tile and perform the flips through scaling
            // This object also contains the tile offset in her transform
            var goCF = new GameObject();
            goCF.name = string.Format("{0} (CF)", superObject.m_TiledName);
            goPivot.AddChildWithUniqueName(goCF);

            goCF.transform.localPosition = translateCenter;
            goCF.transform.localRotation = Quaternion.Euler(0, 0, 0);
            goCF.transform.localScale = new Vector3(flip_h ? -1 : 1, flip_v ? -1 : 1, 1);

            var fromCenter = -translateCenter;

            // Add another child, putting our coordinates back into the proper place
            var goTile = new GameObject(superObject.m_TiledName);
            goCF.AddChildWithUniqueName(goTile);
            goTile.transform.localPosition = fromCenter;
            goTile.transform.localRotation = Quaternion.Euler(0, 0, 0);
            goTile.transform.localScale = Vector3.one;

            if (Importer.SuperImportContext.LayerIgnoreMode != LayerIgnoreMode.Visual)
            {
                // Add the renderer
                var renderer = goTile.AddComponent<SpriteRenderer>();
                renderer.sprite = tile.m_Sprite;
                renderer.color = new Color(1, 1, 1, superObject.CalculateOpacity());
                Importer.AssignMaterial(renderer, m_ObjectLayer.m_TiledName);
                Importer.AssignSpriteSorting(renderer);

                // Add the animator if needed
                if (!tile.m_AnimationSprites.IsEmpty())
                {
                    var tileAnimator = goTile.AddComponent<TileObjectAnimator>();
                    tileAnimator.m_AnimationFramerate = AnimationFramerate;
                    tileAnimator.m_AnimationSprites = tile.m_AnimationSprites;
                }
            }

            if (Importer.SuperImportContext.LayerIgnoreMode != LayerIgnoreMode.Collision)
            {
                // Add any colliders that were set up on the tile in the collision editor
                tile.AddCollidersForTileObject(goTile, Importer.SuperImportContext);
            }

            // Store a reference to our tile object
            superObject.m_SuperTile = tile;
        }

        private void ProcessPolygonElement(GameObject goObject, XElement xPolygon)
        {
            // Get the points of the polygon so we can decompose into a collection of convex polygons
            var points = xPolygon.GetAttributeAsVector2Array("points");
            points = points.Select(p => ColliderFactory.TransformPoint(p)).ToArray();
            points = Importer.SuperImportContext.MakePoints(points);

            // Triangulate the polygon points
            var triangulator = new Triangulator();
            var triangles = triangulator.TriangulatePolygon(points);

            // Gather triangles into a collection of convex polygons
            var composition = new ComposeConvexPolygons();
            var convexPolygons = composition.Compose(triangles);

            PolygonUtils.AddCompositePolygonCollider(goObject, convexPolygons, Importer.SuperImportContext);
        }

        private void ProcessPolylineElement(GameObject goObject, XElement xPolyline)
        {
            var points = xPolyline.GetAttributeAsVector2Array("points");
            ColliderFactory.MakePolyline(goObject, points);
            goObject.AddComponent<SuperColliderComponent>();
        }

        private void ProcessEllipseElement(GameObject goObject, XElement xObject)
        {
            var width = xObject.GetAttributeAs("width", 0f);
            var height = xObject.GetAttributeAs("height", 0f);
            ColliderFactory.MakeEllipse(goObject, width, height);
            goObject.AddComponent<SuperColliderComponent>();
        }

        private void ProcessObjectRectangle(GameObject goObject, XElement xObject)
        {
            var width = xObject.GetAttributeAs("width", 0f);
            var height = xObject.GetAttributeAs("height", 0f);
            ColliderFactory.MakeBox(goObject, width, height);
            goObject.AddComponent<SuperColliderComponent>();
        }

        private void PostProcessObject(GameObject go)
        {
            var properties = go.GetComponent<SuperCustomProperties>();
            var collider = go.GetComponent<Collider2D>();

            if (collider != null)
            {
                CustomProperty isTrigger;
                if (properties.TryGetCustomProperty(StringConstants.Unity_IsTrigger, out isTrigger))
                {
                    collider.isTrigger = Importer.SuperImportContext.GetIsTriggerOverridable(isTrigger.GetValueAsBool());
                }
            }

            // Make sure all children have the same physics layer
            // This is needed for Tile objects in particular that have their colliders in child objects
            go.AssignChildLayers();
        }
    }
}
