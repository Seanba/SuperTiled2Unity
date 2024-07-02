using System;
using UnityEngine;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;


namespace SuperTiled2Unity.Editor
{
    public static class StringExtensions
    {
        private static readonly char[] SeparatorArray = new char[] { '/' };

        public static bool IsNullOrWhiteSpace(this string value)
        {
#if NET_LEGACY
            // From: https://referencesource.microsoft.com/#mscorlib/system/string.cs
            if (value == null) return true;

            for (int i = 0; i < value.Length; i++)
            {
                if (!Char.IsWhiteSpace(value[i])) return false;
            }

            return true;
#else
            return string.IsNullOrWhiteSpace(value);
#endif
        }

        public static byte[] Base64ToBytes(this string data)
        {
            byte[] bytes = Convert.FromBase64String(data);
            return bytes;
        }

        public static void CopyToClipboard(this string str)
        {
            TextEditor te = new TextEditor
            {
                text = str
            };

            te.SelectAll();
            te.Copy();
        }
    }
}
