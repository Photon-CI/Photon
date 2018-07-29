using System.Collections.Generic;

namespace Photon.Framework.Tools.Content
{
    public interface IContentProvider
    {
        char DirectorySeparatorChar {get;}

        IEnumerable<string> GetDirectories(string path);
        IEnumerable<string> GetFiles(string path);
        bool DirectoryExists(string path);
        void CreateDirectory(string path);
    }
}
