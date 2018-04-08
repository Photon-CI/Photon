using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Photon.Framework.Packages
{
    public class ApplicationPackageTools
    {
        /// <summary>
        /// Creates an Application Package using the specified
        /// definition file.
        /// </summary>
        /// <param name="definitionFilename">The file name of the package definition.</param>
        /// <param name="version">The version of the package to create.</param>
        /// <param name="outputFilename">The file name of the output package.</param>
        public static async Task CreatePackage(string definitionFilename, string version, string outputFilename)
        {
            var definition = PackageTools.LoadDefinition<PackageDefinition>(definitionFilename);
            var definitionPath = Path.GetDirectoryName(definitionFilename);

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

                    await PackageTools.AddFiles(archive, definitionPath, fileDefinition.Path, destPath, fileDefinition.Exclude?.ToArray());
                }
            });
        }

        public static async Task<ApplicationPackage> GetMetadata(string filename)
        {
            ApplicationPackage package = null;

            await PackageTools.ReadArchive(filename, async archive => {
                package = await PackageTools.ParseMetadata<ApplicationPackage>(archive);
            });

            return package;
        }

        public static ApplicationPackage Unpack(string package, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            throw new NotImplementedException();
        }

        private static void AppendMetadata(ZipArchive archive, PackageDefinition definition, string version)
        {
            var projectPackage = new ApplicationPackage {
                Id = definition.Id,
                //Name = definition.Name,
                Description = definition.Description,
                //AssemblyFilename = definition.Assembly,
                Version = version,
            };

            PackageTools.AppendMetadata(archive, projectPackage, version);
        }
    }
}
