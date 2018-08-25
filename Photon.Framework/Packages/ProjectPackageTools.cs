using Photon.Framework.Tools;
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
        /// <param name="definition">The project-package definition.</param>
        /// <param name="rootPath">The root path of the package source.</param>
        /// <param name="version">The version of the package to create.</param>
        /// <param name="outputFilename">The filename of the package to create.</param>
        public static async Task CreatePackage(ProjectPackageDefinition definition, string rootPath, string version, string outputFilename)
        {
            var outputFilenameFull = Path.GetFullPath(outputFilename);
            var outputPath = Path.GetDirectoryName(outputFilenameFull);

            if (string.IsNullOrEmpty(outputPath))
                throw new ApplicationException("Empty package output path!");

            PathEx.CreatePath(outputPath);

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
            PathEx.CreatePath(path);

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
                Project = definition.Project,
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
