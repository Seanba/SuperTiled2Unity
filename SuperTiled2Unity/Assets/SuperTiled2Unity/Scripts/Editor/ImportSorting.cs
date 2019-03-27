using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SuperTiled2Unity.Editor
{
    public enum ImportSorting // fixit - rename this and get rid of overhead ones
    {
        Stacking = 0,
        OverheadStatic,
        OverheadDynamic,
        CustomSortAxis,
    }
}
