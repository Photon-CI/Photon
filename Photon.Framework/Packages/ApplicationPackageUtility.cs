using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Framework.Packages
{
    public class ApplicationPackageUtility
    {
        private readonly IDomainContext context;


        public ApplicationPackageUtility(IDomainContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Creates an Application Package using the specified
        /// definition file.
        /// </summary>
        /// <param name="definitionFilename">The file name of the package definition.</param>
        /// <param name="version">The version of the package to create.</param>
        /// <param name="outputFilename">The file name of the output package.</param>
        public async Task CreatePackage(string definitionFilename, string version, string outputFilename)
        {
            var definition = PackageTools.LoadDefinition<PackageDefinition>(definitionFilename);
            var rootPath = Path.GetDirectoryName(definitionFilename);

            context.Output.Append("Packaging ", ConsoleColor.DarkCyan)
                .Append(definition.Id, ConsoleColor.Cyan)
                .Append(" @", ConsoleColor.DarkCyan)
                .Append(version, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            try {
                await ApplicationPackageTools.CreatePackage(definition, rootPath, version, outputFilename);

                context.Output.Append("Packaged ", ConsoleColor.DarkBlue)
                    .Append(definition.Id, ConsoleColor.Blue)
                    .Append(" @", ConsoleColor.DarkBlue)
                    .Append(version, ConsoleColor.Blue)
                    .AppendLine(" successfully.", ConsoleColor.DarkBlue);
            }
            catch (Exception error) {
                context.Output.Append("Failed to package ", ConsoleColor.DarkRed)
                    .Append(definition.Id, ConsoleColor.Red)
                    .Append(" @", ConsoleColor.DarkRed)
                    .Append(version, ConsoleColor.Red)
                    .AppendLine("!", ConsoleColor.DarkRed)
                    .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                throw;
            }
        }

        public async Task<ApplicationPackage> UnpackAsync(string filename, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            ApplicationPackage metadata = null;

            await PackageTools.ReadArchive(filename, async archive => {
                metadata = await PackageTools.ParseMetadataAsync<ApplicationPackage>(archive);

                context.Output.Append("Unpacking ", ConsoleColor.DarkCyan)
                    .Append(metadata.Id, ConsoleColor.Cyan)
                    .Append(" @", ConsoleColor.DarkCyan)
                    .Append(metadata.Version, ConsoleColor.Cyan)
                    .AppendLine("...", ConsoleColor.DarkCyan);

                try {
                    await PackageTools.UnpackBin(archive, path);

                    context.Output.Append("Unpacked ", ConsoleColor.DarkBlue)
                        .Append(metadata.Id, ConsoleColor.Blue)
                        .Append(" @", ConsoleColor.DarkBlue)
                        .Append(metadata.Version, ConsoleColor.Blue)
                        .AppendLine(" successfully.", ConsoleColor.DarkBlue);
                }
                catch (Exception error) {
                    context.Output.Append("Failed to unpack ", ConsoleColor.DarkRed)
                        .Append(metadata.Id, ConsoleColor.Red)
                        .Append(" @", ConsoleColor.DarkRed)
                        .Append(metadata.Version, ConsoleColor.Red)
                        .AppendLine("!", ConsoleColor.DarkRed)
                        .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                    throw;
                }
            });

            return metadata;
        }
    }
}
