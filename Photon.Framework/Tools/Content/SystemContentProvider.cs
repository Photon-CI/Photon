using System.Collections.Generic;
using System.IO;

namespace Photon.Framework.Tools.Content
{
    public class SystemContentProvider : IContentProvider
    {
        public char DirectorySeparatorChar => Path.DirectorySeparatorChar;


        public IEnumerable<string> GetDirectories(string path)
        {
            return Directory.EnumerateDirectories(path);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            return Directory.EnumerateFiles(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }
    }
}
