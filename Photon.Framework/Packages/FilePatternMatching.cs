using System.Collections.Generic;
using System.IO;

namespace Photon.Framework.Packages
{
    static class FilePatternMatching
    {
        public static IEnumerable<KeyValuePair<string, string>> GetFiles(string sourcePath, string destPath, params string[] exclude)
        {
            string rootPath, filterPath;

            var i = sourcePath.IndexOf("*");
            if (i >= 0) {
                var x = sourcePath.Substring(0, i);
                rootPath = Path.GetDirectoryName(x);
                filterPath = sourcePath.Substring(rootPath.Length + 1);
            }
            else {
                rootPath = sourcePath;
                filterPath = "";
            }

            foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)) {
                // TODO: Filter using 'filterPath'

                var file_abs = Path.GetFullPath(file);
                var file_rel = GetRelativePath(file_abs, rootPath);

                if (!string.IsNullOrEmpty(destPath))
                    file_rel = Path.Combine(destPath, file_rel);

                if (exclude != null) {
                    // TODO: Exclusion Filtering!
                }

                yield return new KeyValuePair<string, string>(file, file_rel);
            }
        }

        public static string GetRelativePath(string path, string rootPath)
        {
            return path.StartsWith(rootPath)
                ? path.Substring(rootPath.Length).TrimStart('\\')
                : path;
        }
    }
}
