using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperTiled2Unity
{
    public enum FlipFlags
    {
        None = 0,

        Diagonal = 1,
        Vertical = 2,
        Horizontal = 4,

        // Combos
        D__ = Diagonal,
        DV_ = Diagonal | Vertical,
        D_H = Diagonal | Horizontal,
        DVH = Diagonal | Vertical | Horizontal,
        _V_ = Vertical,
        _VH = Vertical | Horizontal,
        __H = Horizontal,
    }
}
