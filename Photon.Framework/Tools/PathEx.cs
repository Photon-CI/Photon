using System;
using System.IO;

namespace Photon.Framework.Tools
{
    public static class PathEx
    {
        public static void CreateFilePath(string filename)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            var path = Path.GetDirectoryName(filename);

            if (path != null && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static void CreatePath(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
