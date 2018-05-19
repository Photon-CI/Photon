using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Packages
{
    public class ProjectPackageUtility
    {
        private readonly IDomainContext context;

        public string PackageDirectory {get; set;}


        public ProjectPackageUtility(IDomainContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Creates an Project Package using the specified
        /// definition file and pushes it to the server.
        /// </summary>
        /// <param name="definitionFilename">The file name of the package definition.</param>
        /// <param name="version">The version of the package to create.</param>
        public async Task Publish(string definitionFilename, string version, CancellationToken token = default(CancellationToken))
        {
            var name = Path.GetFileName(definitionFilename);

            context.Output.Append("Parsing package definition ", ConsoleColor.DarkCyan)
                .Append(name, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            var definition = PackageTools.LoadDefinition<ProjectPackageDefinition>(definitionFilename);
            var rootPath = Path.GetDirectoryName(definitionFilename);
            var packageFilename = Path.Combine(PackageDirectory, $"{definition.Id}.zip");

            context.Output.Append("Packaging ", ConsoleColor.DarkCyan)
                .Append(definition.Id, ConsoleColor.Cyan)
                .Append(" @", ConsoleColor.DarkCyan)
                .Append(version, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            try {
                await ProjectPackageTools.CreatePackage(definition, rootPath, version, packageFilename);

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

            context.Output.Append("Publishing ", ConsoleColor.DarkCyan)
                .Append(definition.Id, ConsoleColor.Cyan)
                .Append(" @", ConsoleColor.DarkCyan)
                .Append(version, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            try {
                await context.PushProjectPackageAsync(packageFilename, token);

                context.Output.Append("Published ", ConsoleColor.DarkGreen)
                    .Append(definition.Id, ConsoleColor.Green)
                    .Append(" @", ConsoleColor.DarkGreen)
                    .Append(version, ConsoleColor.Green)
                    .AppendLine(" successfully.", ConsoleColor.DarkGreen);
            }
            catch (Exception error) {
                context.Output.Append("Failed to publish ", ConsoleColor.DarkRed)
                    .Append(definition.Id, ConsoleColor.Red)
                    .Append(" @", ConsoleColor.DarkRed)
                    .Append(version, ConsoleColor.Red)
                    .AppendLine("!", ConsoleColor.DarkRed)
                    .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                throw;
            }
        }

        public async Task<ProjectPackage> UnpackAsync(string filename, string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            ProjectPackage metadata = null;

            await PackageTools.ReadArchive(filename, async archive => {
                metadata = await PackageTools.ParseMetadataAsync<ProjectPackage>(archive);

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
