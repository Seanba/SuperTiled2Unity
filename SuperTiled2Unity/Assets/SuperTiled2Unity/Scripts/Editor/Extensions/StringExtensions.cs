using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperTiled2Unity.Editor
{
    public static class StringExtensions
    {
        public static byte[] Base64ToBytes(this string data)
        {
            byte[] bytes = Convert.FromBase64String(data);
            return bytes;
        }

        public static string SanitizePath(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return path.Replace('\\', '/');
        }
    }
}
