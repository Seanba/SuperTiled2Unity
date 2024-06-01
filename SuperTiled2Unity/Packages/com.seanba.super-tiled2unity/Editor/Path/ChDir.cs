using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SuperTiled2Unity.Editor
{
    // Note: It is *very* important you do not call any Unity asset database functions while charning the working directory
    internal class ChDir : IDisposable
    {
        private string m_NewFolderPath;
        private string m_OldFolderPath;

        public static ChDir FromFilename(string filename)
        {
            var directory = Path.GetDirectoryName(filename);
            directory = Path.GetFullPath(directory);
            return new ChDir(directory);
        }

        private ChDir(string absFolderPath)
        {
            m_OldFolderPath = Directory.GetCurrentDirectory();
            m_NewFolderPath = absFolderPath;
            Directory.SetCurrentDirectory(m_NewFolderPath);
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(m_OldFolderPath);
        }
    }
}
