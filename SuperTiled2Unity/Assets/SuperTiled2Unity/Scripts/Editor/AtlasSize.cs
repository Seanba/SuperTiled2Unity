using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperTiled2Unity.Editor
{
    // Only allow certain sizes (and always power-of-two) for atlases used for tiles
    public enum AtlasSize
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192,
    }
}
