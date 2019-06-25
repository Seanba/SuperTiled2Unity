using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SuperTiled2Unity
{
    public class SuperTile : TileBase
    {
        private static readonly Matrix4x4 HorizontalFlipMatrix = MatrixUtils.Rotate2d(-1, 0, 0, 1);
        private static readonly Matrix4x4 VerticalFlipMatrix = MatrixUtils.Rotate2d(1, 0, 0, -1);
        private static readonly Matrix4x4 DiagonalFlipMatrix = MatrixUtils.Rotate2d(0, -1, -1, 0);

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

        public List<CustomProperty> m_CustomProperties;

        public List<CollisionObject> m_CollisionObjects;

        public Matrix4x4 GetTransformMatrix(FlipFlags ff)
        {
            var inversePPU = 1.0f / m_Sprite.pixelsPerUnit;
            var offset = new Vector2(m_TileOffsetX * inversePPU, -m_TileOffsetY * inversePPU);
            var matOffset = Matrix4x4.Translate(offset);

            if (ff == FlipFlags.None)
            {
                return matOffset;
            }

            bool flipHorizontal = (ff & FlipFlags.Horizontal) != 0;
            bool flipVertical = (ff & FlipFlags.Vertical) != 0;
            bool flipDiagonal = (ff & FlipFlags.Diagonal) != 0;

            float width = m_Width * inversePPU;
            float height = m_Height * inversePPU;

            var tileCenter = new Vector2(width, height) * 0.5f;

            Matrix4x4 matTransIn = Matrix4x4.identity;
            Matrix4x4 matFlip = Matrix4x4.identity;
            Matrix4x4 matTransOut = Matrix4x4.identity;

            // Go to the tile center
            matTransIn = Matrix4x4.Translate(-tileCenter);

            // Do the flips
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

            // Go out of the tile center
            if (!flipDiagonal)
            {
                matTransOut = Matrix4x4.Translate(tileCenter);
            }
            else
            {
                float diff = (m_Height - m_Width) * 0.5f * inversePPU;
                tileCenter.x += diff;
                tileCenter.y -= diff;
                matTransOut = Matrix4x4.Translate(tileCenter);
            }

            // Put it all together
            return matOffset * matTransOut * matFlip * matTransIn;
        }

        public void GetTRS(FlipFlags flags, out Vector3 xfTranslate, out Vector3 xfRotate, out Vector3 xfScale)
        {
            float inversePPU = 1.0f / m_Sprite.pixelsPerUnit;
            float width = m_Width * inversePPU;
            float height = m_Height * inversePPU;

            bool flippedHorizontally = FlipFlagsMask.FlippedHorizontally(flags);
            bool flippedVertically = FlipFlagsMask.FlippedVertically(flags);
            bool flippedDiagonally = FlipFlagsMask.FlippedDiagonally(flags);

            xfTranslate = Vector3.zero;
            xfRotate = Vector3.zero;
            xfScale = Vector3.one;

            // fixit - hex cells are different (and don't use diag flag?)

            if (flags != FlipFlags.None)
            {
                if (flippedDiagonally)
                {
                    xfRotate.z = -90.0f;

                    flippedHorizontally = FlipFlagsMask.FlippedVertically(flags);
                    flippedVertically = !FlipFlagsMask.FlippedHorizontally(flags);
                }

                xfScale.x = flippedHorizontally ? -1.0f : 1.0f;
                xfScale.y = flippedVertically ? -1.0f : 1.0f;

                // Mulitply the corners for our tile against rotation and scale matrices to see what our translation should be to get the tile back to the bottom-left origin
                var points = new Vector3[]
                {
                    new Vector3(0, height, 0),
                    new Vector3(width, height, 0),
                    new Vector3(width, 0, 0),
                };

                var matScale = Matrix4x4.Scale(xfScale);
                var matRotate = Matrix4x4.Rotate(Quaternion.Euler(xfRotate));

                points = points.Select(p => matScale.MultiplyPoint(p)).ToArray();
                points = points.Select(p => matRotate.MultiplyPoint(p)).ToArray();

                var minX = points.Select(p => p.x).Min();
                var minY = points.Select(p => p.y).Min();

                xfTranslate.x = -minX;
                xfTranslate.y = -minY;
            }

            // Each tile may have an additional offset
            xfTranslate.x += m_TileOffsetX * inversePPU;
            xfTranslate.y -= m_TileOffsetY * inversePPU;
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = m_Sprite;
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
    }
}
