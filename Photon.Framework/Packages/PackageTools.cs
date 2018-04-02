using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Photon.Framework.Packages
{
    public class PackageTools
    {
        public static async Task CreatePackage(ProjectPackage packageDefinition, string filename)
        {
            //
        }

        public static async Task<string> PackageAsync(string rootPath, string outputPath, PackageDefinition packageDefinition)
        {
            var name = $"{packageDefinition.Id}.{packageDefinition.Version}.zip";
            var filename = Path.Combine(outputPath, name);

            var rootPathAbs = Path.GetFullPath(rootPath);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            WritePackage(filename, archive => {
                // TODO: Add package metadata file

                await AddFiles(archive, rootPath, )

                foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)) {
                    // TODO: Apply Filtering


                    var file_abs = Path.GetFullPath(file);
                    var package_file = GetRelativePath(file_abs, rootPathAbs);
                    var entry = archive.CreateEntry(package_file);

                    using (var fileStream = File.Open(file, FileMode.Open, FileAccess.Read))
                    using (var entryStream = entry.Open()) {
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            });

            //using (var zipStream = File.Open(filename, FileMode.Create, FileAccess.Write))
            //using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, false)) {
            //    foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)) {
            //        // TODO: Apply Filtering

            //        var file_abs = Path.GetFullPath(file);
            //        var package_file = GetRelativePath(file_abs, rootPathAbs);
            //        var entry = archive.CreateEntry(package_file);

            //        using (var fileStream = File.Open(file, FileMode.Open, FileAccess.Read))
            //        using (var entryStream = entry.Open()) {
            //            await fileStream.CopyToAsync(entryStream);
            //        }
            //    }
            //}

            return filename;
        }

        private static async Task WritePackage(string filename, Func<ZipArchive, Task> archiveFunc)
        {
            using (var zipStream = File.Open(filename, FileMode.Create, FileAccess.Write))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, false)) {
                await archiveFunc(archive);
            }
        }

        private static async Task AddFiles(ZipArchive archive, string sourcePath, string destPath = null, PackageFilterCollection filter = null)
        {
            foreach (var file in Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories)) {
                if (filter != null) {
                    if (!filter.IncludesFile(file))
                        continue;
                }

                var file_abs = Path.GetFullPath(file);
                var file_rel = GetRelativePath(file_abs, sourcePath);

                if (!string.IsNullOrEmpty(destPath))
                    file_rel = Path.Combine(destPath, file_rel);

                var entry = archive.CreateEntry(file_rel);

                using (var fileStream = File.Open(file, FileMode.Open, FileAccess.Read))
                using (var entryStream = entry.Open()) {
                    await fileStream.CopyToAsync(entryStream);
                }
            }
        }

        public static void Unpackage(string package, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            throw new NotImplementedException();
        }

        private static string GetRelativePath(string path, string rootPath)
        {
            return path.StartsWith(rootPath)
                ? path.Substring(rootPath.Length)
                : path;
        }
    }
}
