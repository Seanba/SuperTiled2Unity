using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperTiled2Unity.Editor
{
    // Utiltiy class that helps us give items unique names. Must be kept in scope in order to work.
    // Will turn [Name] into [Name (1)], [Name (2)], etc. similar to how Unity gives unique names to similar objects
    public class UniqueNameifier
    {
        private HashSet<string> m_Names = new HashSet<string>();

        public string MakeUniqueName(string name)
        {
            string uniqueName = name;

            int count = 0;
            while (m_Names.Contains(uniqueName, StringComparer.OrdinalIgnoreCase))
            {
                uniqueName = string.Format("{0} ({1})", name, ++count);
            }

            m_Names.Add(uniqueName);

            return uniqueName;
        }

        public static string MakeUniqueHex(string prefix)
        {
            return string.Format("{0}_{1}", prefix, MakeUniqueHex());
        }

        public static string MakeUniqueHex()
        {
            return "0x" + Guid.NewGuid().GetHashCode().ToString("x8");
        }
    }
}
