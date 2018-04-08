using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Photon.Framework.Packages
{
    internal class PackageFileInfo
    {
        public string AbsoluteFilename {get; set;}
        public string RelativeFilename {get; set;}
    }

    internal static class FilePatternMatching
    {
        public static IEnumerable<PackageFileInfo> GetFiles(string rootPath, string sourcePath, string destPath, params string[] exclude)
        {
            var context = new FilterContext {
                SourcePathParts = sourcePath.Split('\\'),
                DestPathParts = sourcePath.Split('\\'),
                ExcludePathsPartsList = exclude
                    .Select(x => x.Split('\\')).ToArray(),
            };

            return context.Filter(rootPath);

            //foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)) {
            //    // TODO: Filter using 'filterPath'

            //    var file_abs = Path.GetFullPath(file);
            //    var file_rel = GetRelativePath(file_abs, rootPath);

            //    if (!string.IsNullOrEmpty(destPath))
            //        file_rel = Path.Combine(destPath, file_rel);

            //    if (exclude != null) {
            //        // TODO: Exclusion Filtering!
            //    }

            //    yield return new KeyValuePair<string, string>(file, file_rel);
            //}
        }

        //public static string GetRelativePath(string path, string rootPath)
        //{
        //    return path.StartsWith(rootPath)
        //        ? path.Substring(rootPath.Length).TrimStart('\\')
        //        : path;
        //}
    }

    internal class FilterContext
    {
        //public string RootPath {get; set;}
        public string[] SourcePathParts {get; set;}
        public string[] DestPathParts {get; set;}
        public string[][] ExcludePathsPartsList {get; set;}
        public bool IsAnySourcePath {get; set;}


        public IEnumerable<PackageFileInfo> Filter(string rootPath)
        {
            var sourcePart = SourcePathParts[0];

            var excludeParts = new List<string>();
            foreach (var excludePathParts in ExcludePathsPartsList) {
                string excludePart = null;
                if (excludePathParts.Length > 0)
                    excludePart = excludePathParts[0];

                excludeParts.Add(excludePart);
            }

            // TODO: break if source matches any exclude

            if (sourcePart == "**") {
                foreach (var y in ScanFolders(rootPath, "*"))
                    yield return y;

                foreach (var y in ScanFiles(rootPath))
                    yield return y;

                //var fileSourcePart = SourcePathParts.Skip(1).FirstOrDefault();
                //var destPath = Path.Combine(DestPathParts);

                //foreach (var file in Directory.EnumerateFiles(rootPath, "*", SearchOption.TopDirectoryOnly)) {
                //    var file_name = Path.GetFileName(file);

                //    // TODO: Match file_name against fileSourcePart

                //    // TODO: Check excludes

                //    yield return new PackageFileInfo {
                //        AbsoluteFilename = file,
                //        RelativeFilename = Path.Combine(destPath, file_name),
                //    };
                //}
            }
            else {
                foreach (var y in ScanFolders(rootPath, sourcePart))
                    yield return y;

                //...
            }
        }

        private IEnumerable<PackageFileInfo> ScanFolders(string rootPath, string pattern)
        {
            foreach (var path in Directory.EnumerateDirectories(rootPath, pattern, SearchOption.TopDirectoryOnly)) {
                var path_name = Path.GetFileName(path);

                var destPathList = DestPathParts.ToList();
                destPathList.Add(path_name);

                var x = new FilterContext {
                    SourcePathParts = SourcePathParts.Skip(1).ToArray(),
                    DestPathParts = destPathList.ToArray(),
                    ExcludePathsPartsList = ExcludePathsPartsList.Select(z => z.Skip(1).ToArray()).ToArray(),
                    IsAnySourcePath = true,
                };

                foreach (var y in x.Filter(path))
                    yield return y;
            }
        }

        private IEnumerable<PackageFileInfo> ScanFiles(string rootPath)
        {
            var fileSourcePart = SourcePathParts.Skip(1).FirstOrDefault();
            var destPath = Path.Combine(DestPathParts);

            foreach (var file in Directory.EnumerateFiles(rootPath, "*", SearchOption.TopDirectoryOnly)) {
                var file_name = Path.GetFileName(file);

                // TODO: Match file_name against fileSourcePart

                // TODO: Check excludes

                yield return new PackageFileInfo {
                    AbsoluteFilename = file,
                    RelativeFilename = Path.Combine(destPath, file_name),
                };
            }
        }
    }
}
