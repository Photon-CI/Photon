using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Photon.Framework.Packages
{
    public class PackageTools
    {
        public static async Task<string> PackageAsync(string rootPath, string outputPath, PackageDefinition packageDefinition)
        {
            var name = $"{packageDefinition.Id}.{packageDefinition.Version}.zip";
            var filename = Path.Combine(outputPath, name);

            var rootPathAbs = Path.GetFullPath(rootPath);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            using (var zipStream = File.Open(filename, FileMode.Create, FileAccess.Write))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, false)) {
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
            }

            return filename;
        }

        public static void Unpackage(string package, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            throw new NotImplementedException();
        }

        private static string GetRelativePath(string path, string rootPath)
        {
            var x = path;

            if (x.StartsWith(rootPath))
                x = x.Substring(rootPath.Length);

            return path.StartsWith(rootPath)
                ? path.Substring(rootPath.Length)
                : path;
        }
    }
}
