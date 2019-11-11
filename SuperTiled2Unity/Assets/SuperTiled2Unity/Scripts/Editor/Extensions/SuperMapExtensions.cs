using UnityEngine;

namespace SuperTiled2Unity.Editor
{
    public static class SuperMapExtensions
    {
        public static Vector2 CellPositionToLocalPosition(this SuperMap superMap, int cx, int cy, SuperImportContext context)
        {
            var grid = superMap.GetComponentInChildren<Grid>();
            var local = grid.CellToLocal(new Vector3Int(cx, cy, 0));

            if (superMap.m_Orientation == MapOrientation.Orthogonal)
            {
                local.y -= grid.cellSize.y;
            }
            else if (superMap.m_Orientation == MapOrientation.Isometric)
            {
                local.x -= grid.cellSize.x * 0.5f;
                local.y -= grid.cellSize.y;
            }
            else if (superMap.m_Orientation == MapOrientation.Staggered)
            {
                if (superMap.m_StaggerAxis == StaggerAxis.Y)
                {
                    local.y -= grid.cellSize.y;

                    if (superMap.m_StaggerIndex == StaggerIndex.Even)
                    {
                        local.x += grid.cellSize.x * 0.5f;
                    }
                }
                else
                {
                    if (superMap.m_StaggerIndex == StaggerIndex.Odd)
                    {
                        local.y -= grid.cellSize.y;
                    }
                    else
                    {
                        local.y -= grid.cellSize.y * 1.5f;
                    }
                }
            }
            else if (superMap.m_Orientation == MapOrientation.Hexagonal)
            {
                if (superMap.m_StaggerAxis == StaggerAxis.X && superMap.m_StaggerIndex == StaggerIndex.Odd)
                {
                    local.x -= grid.cellSize.y * 0.75f;
                    local.y -= grid.cellSize.x * 1.5f;
                }
                else if (superMap.m_StaggerAxis == StaggerAxis.X && superMap.m_StaggerIndex == StaggerIndex.Even)
                {
                    local.y -= grid.cellSize.x * 1.5f;
                }
                else if (superMap.m_StaggerAxis == StaggerAxis.Y && superMap.m_StaggerIndex == StaggerIndex.Odd)
                {
                    local.y -= grid.cellSize.y;
                }
                else
                {
                    // Y-Even
                    local.y -= grid.cellSize.y * 0.25f;
                }
            }

            return local;
        }
    }
}
