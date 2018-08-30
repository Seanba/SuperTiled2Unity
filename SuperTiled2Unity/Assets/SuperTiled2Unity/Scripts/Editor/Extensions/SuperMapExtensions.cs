using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperTiled2Unity.Editor
{
    public static class SuperMapExtensions
    {
        public static void UpdateProperties(this SuperMap superMap, SuperImportContext importContext)
        {
            var cellSize = superMap.CalculateCellSize();

            // The cell size has to take pixels per unit into account
            superMap.CellSize = importContext.MakeSize(cellSize);
        }
    }
}
