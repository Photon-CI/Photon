using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Photon.Framework.Packages
{
    public class ProjectPackageTools
    {
        public static ProjectPackageDefinition LoadDefinition(string filename)
        {
            return PackageTools.LoadDefinition<ProjectPackageDefinition>(filename);
        }

        /// <summary>
        /// Creates a Project Package using the specified
        /// definition file.
        /// </summary>
        /// <param name="version">The version of the package to create.</param>
        /// <param name="outputFilename">The file name of the output package.</param>
        public static async Task CreatePackage(ProjectPackageDefinition definition, string rootPath, string version, string outputFilename)
        {

            var outputFilenameFull = Path.GetFullPath(outputFilename);
            var outputPath = Path.GetDirectoryName(outputFilenameFull);

            if (string.IsNullOrEmpty(outputPath))
                throw new ApplicationException("Empty package output path!");

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            await PackageTools.WriteArchive(outputFilename, async archive => {
                AppendMetadata(archive, definition, version);

                foreach (var fileDefinition in definition.Files) {
                    var destPath = Path.Combine("bin", fileDefinition.Destination);

                    await PackageTools.AddFiles(archive, rootPath, fileDefinition.Path, destPath, fileDefinition.Exclude?.ToArray());
                }
            });
        }

        public static async Task<ProjectPackage> GetMetadataAsync(string filename)
        {
            ProjectPackage package = null;

            await PackageTools.ReadArchive(filename, async archive => {
                package = await PackageTools.ParseMetadataAsync<ProjectPackage>(archive);
            });

            return package;
        }

        public static async Task<ProjectPackage> UnpackAsync(string filename, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            ProjectPackage metadata = null;

            await PackageTools.ReadArchive(filename, async archive => {
                metadata = await PackageTools.ParseMetadataAsync<ProjectPackage>(archive);

                await PackageTools.UnpackBin(archive, path);
            });

            return metadata;
        }

        private static void AppendMetadata(ZipArchive archive, ProjectPackageDefinition definition, string version)
        {
            var metadata = new ProjectPackage {
                Id = definition.Id,
                Name = definition.Name,
                Description = definition.Description,
                AssemblyFilename = definition.Assembly,
                ScriptName = definition.Script,
                Version = version,
            };

            PackageTools.AppendMetadata(archive, metadata, version);
        }
    }
}
