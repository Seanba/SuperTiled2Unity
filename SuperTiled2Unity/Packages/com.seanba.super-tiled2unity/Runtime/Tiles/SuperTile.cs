using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity
{
    public class SuperTile : TileBase
    {
        private static readonly Matrix4x4 HorizontalFlipMatrix = MatrixUtils.Rotate2d(-1, 0, 0, 1);
        private static readonly Matrix4x4 VerticalFlipMatrix = MatrixUtils.Rotate2d(1, 0, 0, -1);
        private static readonly Matrix4x4 DiagonalFlipMatrix = MatrixUtils.Rotate2d(0, -1, -1, 0);
        private static readonly Matrix4x4 Rotate60Matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, -60));
        private static readonly Matrix4x4 Rotate120Matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, -120));

        public static SuperTile CreateSuperTile()
        {
            return CreateInstance<SuperTile>();
        }

        [ReadOnly]
        public int m_TileId;

        [ReadOnly]
        public Sprite m_Sprite;

        [ReadOnly]
        public Sprite[] m_AnimationSprites;

        [ReadOnly]
        public string m_Type;

        [ReadOnly]
        public float m_Width;

        [ReadOnly]
        public float m_Height;

        [ReadOnly]
        public float m_TileOffsetX;

        [ReadOnly]
        public float m_TileOffsetY;

        [ReadOnly]
        public ObjectAlignment m_ObjectAlignment;

        [ReadOnly]
        public TileRenderSize m_TileRenderSize;

        [ReadOnly]
        public FillMode m_FillMode;

        [ReadOnly]
        public Tile.ColliderType m_ColliderType;

        public List<CustomProperty> m_CustomProperties;

        public List<CollisionObject> m_CollisionObjects;

        public Matrix4x4 GetTransformMatrix(FlipFlags flipFlags, SuperMap superMap)
        {
            if (m_TileRenderSize == TileRenderSize.Tile && m_TileOffsetX == 0 && m_TileOffsetY == 0 && flipFlags == FlipFlags.None)
            {
                // No special transform needed for this tile
                return Matrix4x4.identity;
            }

            var matOffset = CalculateTileOffsetMatrix();
            var matFlip = CacluateFlipMatrix(flipFlags, superMap);
            var matRenderSize = CalculateRenderSizeMatrix(superMap);

            return matOffset * matRenderSize * matFlip;
        }

        public void GetTRS(FlipFlags flags, MapOrientation orientation, SuperMap superMap, out Vector3 xfTranslate, out Vector3 xfRotate, out Vector3 xfScale)
        {
            float inversePPU = 1.0f / m_Sprite.pixelsPerUnit;
            float width = m_Width * inversePPU;
            float height = m_Height * inversePPU;

            bool flippedHorizontally = FlipFlagsMask.FlippedHorizontally(flags);
            bool flippedVertically = FlipFlagsMask.FlippedVertically(flags);
            bool rotatedDiagonally = FlipFlagsMask.RotatedDiagonally(flags);
            bool rotatedHexagonally120 = FlipFlagsMask.RotatedHexagonally120(flags);

            xfTranslate = Vector3.zero;
            xfRotate = Vector3.zero;
            xfScale = Vector3.one;

            if (flags != FlipFlags.None)
            {
                if (orientation == MapOrientation.Hexagonal)
                {
                    if (rotatedDiagonally)
                    {
                        xfRotate.z -= 60;
                    }

                    if (rotatedHexagonally120)
                    {
                        xfRotate.z -= 120;
                    }
                }
                else if (rotatedDiagonally)
                {
                    xfRotate.z = -90.0f;

                    flippedHorizontally = FlipFlagsMask.FlippedVertically(flags);
                    flippedVertically = !FlipFlagsMask.FlippedHorizontally(flags);
                }

                xfScale.x = flippedHorizontally ? -1.0f : 1.0f;
                xfScale.y = flippedVertically ? -1.0f : 1.0f;

                var matScale = Matrix4x4.Scale(xfScale);
                var matRotate = Matrix4x4.Rotate(Quaternion.Euler(xfRotate));

                if (orientation == MapOrientation.Hexagonal)
                {
                    // Hex tiles use the center of the tile for transforms
                    var anchor = new Vector3(width * 0.5f, height * 0.5f);

                    var transformed = matScale.MultiplyPoint(anchor);
                    transformed = matRotate.MultiplyPoint(transformed);

                    xfTranslate.x = anchor.x - transformed.x;
                    xfTranslate.y = anchor.y - transformed.y;
                }
                else
                {
                    // Mulitply the corners for our tile against rotation and scale matrices to see what our translation should be to get the tile back to the bottom-left origin
                    var points = new Vector3[]
                    {
                        new Vector3(0, height, 0),
                        new Vector3(width, height, 0),
                        new Vector3(width, 0, 0),
                    };

                    points = points.Select(p => matScale.MultiplyPoint(p)).ToArray();
                    points = points.Select(p => matRotate.MultiplyPoint(p)).ToArray();

                    var minX = points.Select(p => p.x).Min();
                    var minY = points.Select(p => p.y).Min();

                    xfTranslate.x = -minX;
                    xfTranslate.y = -minY;
                }
            }

            // Each tile may have an additional offset
            xfTranslate.x += m_TileOffsetX * inversePPU;
            xfTranslate.y -= m_TileOffsetY * inversePPU;

            if (m_TileRenderSize == TileRenderSize.Grid)
            {
                // Additional translation/scales needed for grid render size
                float mapTileWidth = superMap.m_TileWidth * inversePPU;
                float mapTileHeight = superMap.m_TileHeight * inversePPU;
                float scale_w = mapTileWidth / width;
                float scale_h = mapTileHeight / height;

                if (m_FillMode == FillMode.Stretch)
                {
                    // The tile should be scaled to fill the grid cell
                    xfScale.x *= scale_w;
                    xfScale.y *= scale_h;
                }
                else if (m_FillMode == FillMode.Preserve_Aspect_Fit)
                {
                    // Scale the tile by the minimum amount needed to fill the grid cell along one axis
                    float scale = Mathf.Min(scale_w, scale_h);
                    xfScale.x *= scale;
                    xfScale.y *= scale;

                    // We also have to offset the tile so that is centered in the grid
                    float offset_x = (mapTileWidth - scale * width) * 0.5f;
                    float offset_y = (mapTileHeight - scale * height) * 0.5f;
                    xfTranslate.x += offset_x;
                    xfTranslate.y -= offset_y;
                }
            }
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = m_Sprite;
            tileData.colliderType = m_ColliderType;
        }

        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            if (m_AnimationSprites != null && m_AnimationSprites.Length > 1)
            {
                tileAnimationData.animatedSprites = m_AnimationSprites;
                tileAnimationData.animationSpeed = 1.0f;
                tileAnimationData.animationStartTime = 0;
                return true;
            }
            return false;
        }

        private Matrix4x4 CalculateTileOffsetMatrix()
        {
            float inversePPU = 1.0f / m_Sprite.pixelsPerUnit;
            var offset = new Vector2(m_TileOffsetX * inversePPU, -m_TileOffsetY * inversePPU);
            var matOffset = Matrix4x4.Translate(offset);
            return matOffset;
        }

        private Matrix4x4 CalculateRenderSizeMatrix(SuperMap superMap)
        {
            if (m_TileRenderSize == TileRenderSize.Tile)
            {
                // No need to offset and/or scale the tile to fit into the map grid
                return Matrix4x4.identity;
            }

            // Todo: Render size'd tiles don't look right when they are rotated but I don't think they look right in Tiled either

            var matRenderSizeOffset = Matrix4x4.identity;
            var matRenderSizeScale = Matrix4x4.identity;

            float inversePPU = 1.0f / m_Sprite.pixelsPerUnit;
            float mapTileWidth = superMap.m_TileWidth * inversePPU;
            float mapTileHeight = superMap.m_TileHeight * inversePPU;
            float width = m_Width * inversePPU;
            float height = m_Height * inversePPU;

            float scale_w = mapTileWidth / width;
            float scale_h = mapTileHeight / height;

            if (m_FillMode == FillMode.Stretch)
            {
                // The tile should be scaled to fill the grid cell
                matRenderSizeScale = Matrix4x4.Scale(new Vector3(scale_w, scale_h, 1));
            }
            else if (m_FillMode == FillMode.Preserve_Aspect_Fit)
            {
                // Scale the tile by the minimum amount needed to fill the grid cell along one axis
                float scale = Mathf.Min(scale_w, scale_h);
                matRenderSizeScale = Matrix4x4.Scale(new Vector3(scale, scale, 1));

                // We also have to offset the tile so that is centered in the grid
                float offset_x = (mapTileWidth - scale * width) * 0.5f;
                float offset_y = (mapTileHeight - scale * height) * 0.5f;
                matRenderSizeOffset = Matrix4x4.Translate(new Vector3(offset_x, -offset_y, 0));
            }

            return matRenderSizeOffset * matRenderSizeScale;
        }

        private Matrix4x4 CacluateFlipMatrix(FlipFlags flags, SuperMap superMap)
        {
            if (flags == FlipFlags.None)
            {
                // Not flipping
                return Matrix4x4.identity;
            }

            float inversePPU = 1.0f / m_Sprite.pixelsPerUnit;

            bool flipHorizontal = FlipFlagsMask.FlippedHorizontally(flags);
            bool flipVertical = FlipFlagsMask.FlippedVertically(flags);
            bool flipDiagonal = FlipFlagsMask.RotatedDiagonally(flags);
            bool rotateHex120 = FlipFlagsMask.RotatedHexagonally120(flags);

            float width = m_Width * inversePPU;
            float height = m_Height * inversePPU;
            var tileCenter = new Vector2(width, height) * 0.5f;

            Matrix4x4 matTransIn = Matrix4x4.identity;
            Matrix4x4 matFlip = Matrix4x4.identity;
            Matrix4x4 matTransOut = Matrix4x4.identity;

            // Go to the tile center
            matTransIn = Matrix4x4.Translate(-tileCenter);

            if (superMap.m_Orientation == MapOrientation.Hexagonal)
            {
                if (flipDiagonal)
                {
                    matFlip *= Rotate60Matrix;
                }

                if (rotateHex120)
                {
                    matFlip *= Rotate120Matrix;
                }

                if (flipHorizontal)
                {
                    matFlip *= HorizontalFlipMatrix;
                }

                if (flipVertical)
                {
                    matFlip *= VerticalFlipMatrix;
                }

                matTransOut = Matrix4x4.Translate(tileCenter);
            }
            else
            {
                if (flipHorizontal)
                {
                    matFlip *= HorizontalFlipMatrix;
                }

                if (flipVertical)
                {
                    matFlip *= VerticalFlipMatrix;
                }

                if (flipDiagonal)
                {
                    matFlip *= DiagonalFlipMatrix;
                }

                // Go off of the tile center
                if (!flipDiagonal)
                {
                    matTransOut = Matrix4x4.Translate(tileCenter);
                }
                else
                {
                    // Compensate for width and height being different dimensions
                    float diff = (height - width) * 0.5f;
                    tileCenter.x += diff;
                    tileCenter.y -= diff;
                    matTransOut = Matrix4x4.Translate(tileCenter);
                }
            }

            return matTransOut * matFlip * matTransIn;
        }
    }
}
