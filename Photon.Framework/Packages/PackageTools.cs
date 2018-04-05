using Newtonsoft.Json;
using Photon.Framework.Extensions;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Photon.Framework.Packages
{
    public class PackageTools
    {
        private const string PackageBin = "bin";

        /// <summary>
        /// Creates a Project Package using the specified
        /// definition file.
        /// </summary>
        /// <param name="definitionFilename">The file name of the package definition.</param>
        /// <param name="outputFilename">The file name of the output package.</param>
        public static async Task CreateProjectPackage(string definitionFilename, string version, string outputFilename)
        {
            var definition = LoadPackageDefinition(definitionFilename);
            var definitionPath = Path.GetDirectoryName(definitionFilename);

            var outputPath = Path.GetDirectoryName(outputFilename);

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            await WritePackageArchive(outputFilename, async archive => {
                AppendPackageMetadata(archive, definition, version);

                foreach (var fileDefinition in definition.Files) {
                    var sourcePath = Path.Combine(definitionPath, fileDefinition.Path);

                    var destPath = PackageBin;

                    if (fileDefinition.Destination != null)
                        destPath = Path.Combine(destPath, fileDefinition.Destination);
                    else {
                        var destPathRel = Path.GetDirectoryName(fileDefinition.Path);
                        destPath = Path.Combine(destPath, destPathRel);
                    }

                    await AddFiles(archive, sourcePath, destPath, fileDefinition.Exclude?.ToArray());
                }
            });
        }

        public static ProjectPackage UnpackProject(string package, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            throw new NotImplementedException();
        }


        public static ProjectPackage UnpackApplication(string package, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            throw new NotImplementedException();
        }

        private static async Task WritePackageArchive(string filename, Func<ZipArchive, Task> archiveFunc)
        {
            using (var zipStream = File.Open(filename, FileMode.Create, FileAccess.Write))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, false)) {
                await archiveFunc(archive);
            }
        }

        private static PackageDefinition LoadPackageDefinition(string filename)
        {
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<PackageDefinition>(stream);
            }
        }

        private static void AppendPackageMetadata(ZipArchive archive, PackageDefinition definition, string version)
        {
            var projectPackage = new ProjectPackage {
                Id = definition.Id,
                Name = definition.Name,
                Description = definition.Description,
                AssemblyFilename = definition.Assembly,
                Version = version,
            };

            var entry = archive.CreateEntry("metadata.json");

            using (var entryStream = entry.Open()) {
                var serializer = new JsonSerializer();
                serializer.Serialize(entryStream, projectPackage);
            }
        }

        private static async Task AddFiles(ZipArchive archive, string sourcePath, string destPath, string[] exclude = null)
        {
            foreach (var file in FilePatternMatching.GetFiles(sourcePath, destPath, exclude)) {
                var entry = archive.CreateEntry(file.Value);

                using (var fileStream = File.Open(file.Key, FileMode.Open, FileAccess.Read))
                using (var entryStream = entry.Open()) {
                    await fileStream.CopyToAsync(entryStream);
                }
            }
        }
    }
}
