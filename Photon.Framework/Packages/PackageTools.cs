using Photon.Framework.Extensions;
using Photon.Framework.Tools;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Framework.Packages
{
    internal static class PackageTools
    {
        public static async Task WriteArchive(string filename, Func<ZipArchive, Task> archiveFunc)
        {
            using (var zipStream = File.Open(filename, FileMode.Create, FileAccess.Write))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, false)) {
                await archiveFunc(archive);
            }
        }

        public static async Task ReadArchive(string filename, Func<ZipArchive, Task> archiveFunc)
        {
            using (var zipStream = File.Open(filename, FileMode.Open, FileAccess.Read))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, false)) {
                await archiveFunc(archive);
            }
        }

        public static T LoadDefinition<T>(string filename)
            where T : IPackageDefinition
        {
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                return JsonSettings.Serializer.Deserialize<T>(stream);
            }
        }

        public static void AppendMetadata<T>(ZipArchive archive, T definition, string version)
            where T : IPackageMetadata
        {
            var entry = archive.CreateEntry("metadata.json");

            using (var entryStream = entry.Open()) {
                JsonSettings.Serializer.Serialize(entryStream, definition);
            }
        }

        public static async Task<T> ParseMetadataAsync<T>(ZipArchive archive)
            where T : IPackageMetadata
        {
            var entry = archive.GetEntry("metadata.json");
            if (entry == null) throw new Exception("Project Package metadata.json not found!");

            using (var entryStream = entry.Open())
            using (var dataStream = new MemoryStream()) {
                await entryStream.CopyToAsync(dataStream);
                await dataStream.FlushAsync();

                dataStream.Seek(0, SeekOrigin.Begin);

                return JsonSettings.Serializer.Deserialize<T>(dataStream);
            }
        }

        public static async Task AddFiles(ZipArchive archive, string rootPath, string sourcePath, string destPath, string[] exclude = null)
        {
            foreach (var file in FilePatternMatching.GetFiles(rootPath, sourcePath, destPath, exclude)) {
                var entry = archive.CreateEntry(file.RelativeFilename);

                using (var fileStream = File.Open(file.AbsoluteFilename, FileMode.Open, FileAccess.Read))
                using (var entryStream = entry.Open()) {
                    await fileStream.CopyToAsync(entryStream);
                }
            }
        }

        public static async Task UnpackBin(ZipArchive archive, string destPath)
        {
            foreach (var entry in archive.Entries) {
                var entryPath = Path.GetDirectoryName(entry.FullName) ?? string.Empty;
                var entryPathParts = entryPath.Split(Path.DirectorySeparatorChar);
                var entryPathRoot = entryPathParts.FirstOrDefault();

                if (!string.Equals(entryPathRoot, "bin")) continue;

                var entryPathDestParts = entryPathParts.Skip(1).ToArray();
                var entryPathDest = Path.Combine(entryPathDestParts);

                var entryDestPath = Path.Combine(destPath, entryPathDest);
                var entryDestFilename = Path.Combine(entryDestPath, entry.Name);

                PathEx.CreatePath(entryDestPath);

                using (var entryStream = entry.Open())
                using (var fileStream = File.Open(entryDestFilename, FileMode.Create, FileAccess.Write)) {
                    await entryStream.CopyToAsync(fileStream);
                }
            }
        }
    }
}
