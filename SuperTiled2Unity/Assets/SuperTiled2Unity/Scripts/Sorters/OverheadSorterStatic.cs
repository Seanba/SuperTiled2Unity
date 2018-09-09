using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity
{
    // Only sort the sprite on start up
    public class OverheadSorterStatic : OverheadSorter
    {
        private void Start()
        {
            SortOnYPosition();
        }
    }
}
