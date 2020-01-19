using System.IO;

namespace SuperTiled2Unity.Editor
{
    public static class StreamExtensions
    {
        public static long CopyTo(this Stream source, Stream destination)
        {
            byte[] buffer = new byte[1048 * 16];
            int bytesRead;
            long totalBytes = 0;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
                totalBytes += bytesRead;
            }
            return totalBytes;
        }
    }
}
