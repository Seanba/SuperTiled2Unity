using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperTiled2Unity.Editor
{
    public class AssetDependencies
    {
        private string m_AssetPath;
        private List<string> m_Dependencies = new List<string>();
        private List<string> m_References = new List<string>();

        public string AssetPath { get { return m_AssetPath; } }
        public IEnumerable<string> Dependencies { get { return m_Dependencies; } }
        public IEnumerable<string> References { get { return m_References; } }

        public AssetDependencies(string assetPath)
        {
            m_AssetPath = assetPath;
        }

        public void AssignDependencies(IEnumerable<string> assetPaths)
        {
            m_Dependencies = assetPaths.ToList();
        }

        public void AddReference(string path)
        {
            if (!m_References.Contains(path, StringComparer.OrdinalIgnoreCase))
            {
                m_References.Add(path);
            }
        }

        public void RemoveReference(string path)
        {
            m_References.RemoveAll(r => r.Equals(path, StringComparison.OrdinalIgnoreCase));
        }
    }
}
