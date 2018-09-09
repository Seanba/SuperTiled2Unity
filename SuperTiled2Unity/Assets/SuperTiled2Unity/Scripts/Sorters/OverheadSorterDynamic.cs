using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity
{
    // Sort the sprite based on their y position every frame if they have moved
    public class OverheadSorterDynamic : OverheadSorter
    {
        private void LateUpdate()
        {
            if (transform.hasChanged)
            {
                SortOnYPosition();
                transform.hasChanged = false;
            }
        }
    }
}
