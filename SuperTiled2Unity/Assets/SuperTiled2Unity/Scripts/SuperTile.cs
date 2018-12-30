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

        public void GetTRS(FlipFlags flags, out Vector3 trans, out Vector3 rot, out Vector3 scale)
        {
            float inversePPU = 1.0f / m_Sprite.pixelsPerUnit;
            float width = m_Width * inversePPU;
            float height = m_Height * inversePPU;

            switch (flags)
            {
                // diagonal
                case FlipFlags.D__:
                    trans = new Vector3(height, width, 0);
                    rot = new Vector3(0, 0, -90);
                    scale = new Vector3(1, -1, 1);
                    break;

                // diagonal-vertical
                case FlipFlags.DV_:
                    trans = new Vector3(height, 0, 0);
                    rot = new Vector3(0, 0, 90);
                    scale = Vector3.one;
                    break;

                // diagonal-horizontal
                case FlipFlags.D_H:
                    trans = new Vector3(0, width, 0);
                    rot = new Vector3(0, 0, -90);
                    scale = Vector3.one;
                    break;

                // diagonal-vertical-horizontal
                case FlipFlags.DVH:
                    trans = Vector3.zero;
                    rot = new Vector3(0, 0, 90);
                    scale = new Vector3(1, -1, 1);
                    break;

                // vertical
                case FlipFlags._V_:
                    trans = new Vector3(0, height, 0);
                    rot = Vector3.zero;
                    scale = new Vector3(1, -1, 1);
                    break;

                // vertical-horizontal
                case FlipFlags._VH:
                    trans = new Vector3(width, height, 0);
                    rot = Vector3.zero;
                    scale = new Vector3(-1, -1, 1);
                    break;

                // horizontal
                case FlipFlags.__H:
                    trans = new Vector3(width, 0, 0);
                    rot = Vector3.zero;
                    scale = new Vector3(-1, 1, 1);
                    break;

                default:
                    trans = Vector3.zero;
                    rot = Vector3.zero;
                    scale = Vector3.one;
                    break;
            }

            // Each tile may have an additional offset
            trans.x += m_TileOffsetX * inversePPU;
            trans.y -= m_TileOffsetY * inversePPU;
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.flags = TileFlags.LockAll;
            tileData.sprite = m_Sprite;

            var flags = (FlipFlags)position.z;
            tileData.transform = GetTransformMatrix(flags);
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
