using Photon.Framework.Tools.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Photon.Tests.Content
{
    internal class MockContentProvider : IContentProvider
    {
        public char DirectorySeparatorChar => '\\';

        public StringComparison Comparer {get; set;}
        public List<string> Directories {get; set;}
        public List<string> Files {get; set;}


        public MockContentProvider()
        {
            Comparer = StringComparison.OrdinalIgnoreCase;
            Directories = new List<string>();
            Files = new List<string>();
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            return Directories.Where(x => {
                var directoryPath = Path.GetDirectoryName(x) ?? string.Empty;
                return string.Equals(directoryPath, path, Comparer);
            });
        }

        public IEnumerable<string> GetFiles(string path)
        {
            return Files.Where(x => {
                var directoryPath = Path.GetDirectoryName(x) ?? string.Empty;
                return string.Equals(directoryPath, path, Comparer);
            });
        }

        public bool DirectoryExists(string path)
        {
            return Directories.Any(x => string.Equals(x, path, Comparer));
        }

        public void CreateDirectory(string path)
        {
            Directories.Add(path);
        }
    }
}
