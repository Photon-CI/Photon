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
            var sourceParts = sourcePath.Split('\\');

            var anyPathIndex = -1;
            for (var i = 0; i < sourceParts.Length; i++) {
                if (sourceParts[i] == "**") {
                    anyPathIndex = i;
                    break;
                }
            }

            if (anyPathIndex >= 0) {
                var rootParts = sourceParts.Take(anyPathIndex).ToArray();

                var filePart = sourceParts.Skip(anyPathIndex + 1).ToArray();

                foreach (var path in GetPaths(rootPath, string.Empty, rootParts)) {
                    var pathAbs = Path.Combine(rootPath, path);

                    if (filePart.Length > 1) {
                        foreach (var file in GetPathFiles(pathAbs, string.Empty, filePart)) {
                            yield return new PackageFileInfo {
                                AbsoluteFilename = Path.Combine(pathAbs, file),
                                RelativeFilename = Path.Combine(destPath, file),
                            };
                        }
                    }
                    else {
                        foreach (var subPath in GetAllPaths(pathAbs, string.Empty)) {
                            var subPathName = Path.GetFileName(subPath) ?? string.Empty;
                            var subPathAbs = Path.Combine(pathAbs, subPath);

                            foreach (var file in GetPathFiles(subPathAbs, string.Empty, filePart)) {
                                yield return new PackageFileInfo {
                                    AbsoluteFilename = Path.Combine(subPathAbs, file),
                                    RelativeFilename = Path.Combine(destPath, subPathName, file),
                                };
                            }
                        }

                        foreach (var file in GetPathFiles(pathAbs, string.Empty, filePart)) {
                            yield return new PackageFileInfo {
                                AbsoluteFilename = Path.Combine(pathAbs, file),
                                RelativeFilename = Path.Combine(destPath, file),
                            };
                        }
                    }
                }
            }
            else {
                var rootParts = sourceParts.Length > 0
                    ? sourceParts.Take(sourceParts.Length - 1).ToArray()
                    : new string[0];

                var filePart = new[] {sourceParts.LastOrDefault()};

                if (rootParts.Length > 0) {
                    foreach (var path in GetPaths(rootPath, string.Empty, rootParts)) {
                        var pathAbs = Path.Combine(rootPath, path);

                        foreach (var file in GetPathFiles(pathAbs, string.Empty, filePart)) {
                            yield return new PackageFileInfo {
                                AbsoluteFilename = Path.Combine(pathAbs, file),
                                RelativeFilename = Path.Combine(destPath, file),
                            };
                        }
                    }
                }
                else {
                    foreach (var file in GetPathFiles(rootPath, string.Empty, filePart)) {
                        yield return new PackageFileInfo {
                            AbsoluteFilename = Path.Combine(rootPath, file),
                            RelativeFilename = Path.Combine(destPath, file),
                        };
                    }
                }
            }
        }

        private static IEnumerable<string> GetAllPaths(string rootPath, string destPath)
        {
            foreach (var path in Directory.EnumerateDirectories(rootPath, "*", SearchOption.TopDirectoryOnly)) {
                var pathName = Path.GetFileName(path) ?? string.Empty;
                var subDestPath = Path.Combine(destPath, pathName);
                yield return subDestPath;

                foreach (var subPath in GetAllPaths(path, subDestPath))
                    yield return subPath;
            }
        }

        private static IEnumerable<string> GetPaths(string rootPath, string destPath, string[] pathParts)
        {
            foreach (var path in Directory.EnumerateDirectories(rootPath, pathParts[0], SearchOption.TopDirectoryOnly)) {
                var pathName = Path.GetFileName(path) ?? string.Empty;
                var subDestPath = Path.Combine(destPath, pathName);

                if (pathParts.Length <= 1) {
                    yield return subDestPath;
                }
                else {
                    foreach (var subPath in GetPaths(path, subDestPath, pathParts.Skip(1).ToArray())) {
                        yield return subPath;
                    }
                }
            }
        }

        private static IEnumerable<string> GetPathFiles(string rootPath, string destPath, string[] pathParts)
        {
            var pattern = pathParts.FirstOrDefault() ?? "*";

            foreach (var file in Directory.EnumerateFiles(rootPath, pattern, SearchOption.TopDirectoryOnly)) {
                var name = Path.GetFileName(file) ?? string.Empty;

                yield return Path.Combine(destPath, name);
            }
        }
    }
}
