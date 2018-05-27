using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using Photon.Framework.Tools;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Packages
{
    public class ApplicationPackageUtility
    {
        private readonly IDomainContext context;

        public string PackageDirectory {get; set;}


        public ApplicationPackageUtility(IDomainContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Creates an Application Package using the specified
        /// definition file and pushes it to the server.
        /// </summary>
        /// <param name="definitionFilename">The file name of the package definition.</param>
        /// <param name="version">The version of the package to create.</param>
        public async Task Publish(string definitionFilename, string version, CancellationToken token = default(CancellationToken))
        {
            var name = Path.GetFileName(definitionFilename);

            context.Output.Write("Parsing package definition ", ConsoleColor.DarkCyan)
                .Write(name, ConsoleColor.Cyan)
                .WriteLine("...", ConsoleColor.DarkCyan);

            var definition = PackageTools.LoadDefinition<ApplicationPackageDefinition>(definitionFilename);
            var rootPath = Path.GetDirectoryName(definitionFilename);
            var packageFilename = Path.Combine(PackageDirectory, $"{definition.Id}.zip");

            context.Output.Write("Packaging ", ConsoleColor.DarkCyan)
                .Write(definition.Id, ConsoleColor.Cyan)
                .Write(" @", ConsoleColor.DarkCyan)
                .Write(version, ConsoleColor.Cyan)
                .WriteLine("...", ConsoleColor.DarkCyan);

            try {
                await ApplicationPackageTools.CreatePackage(definition, rootPath, version, packageFilename);

                context.Output.Write("Packaged ", ConsoleColor.DarkBlue)
                    .Write(definition.Id, ConsoleColor.Blue)
                    .Write(" @", ConsoleColor.DarkBlue)
                    .Write(version, ConsoleColor.Blue)
                    .WriteLine(" successfully.", ConsoleColor.DarkBlue);
            }
            catch (Exception error) {
                context.Output.Write("Failed to package ", ConsoleColor.DarkRed)
                    .Write(definition.Id, ConsoleColor.Red)
                    .Write(" @", ConsoleColor.DarkRed)
                    .Write(version, ConsoleColor.Red)
                    .WriteLine("!", ConsoleColor.DarkRed)
                    .WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                throw;
            }

            context.Output.Write("Publishing ", ConsoleColor.DarkCyan)
                .Write(definition.Id, ConsoleColor.Cyan)
                .Write(" @", ConsoleColor.DarkCyan)
                .Write(version, ConsoleColor.Cyan)
                .WriteLine("...", ConsoleColor.DarkCyan);

            try {
                await context.PushApplicationPackageAsync(packageFilename, token);

                context.Output.Write("Published ", ConsoleColor.DarkGreen)
                    .Write(definition.Id, ConsoleColor.Green)
                    .Write(" @", ConsoleColor.DarkGreen)
                    .Write(version, ConsoleColor.Green)
                    .WriteLine(" successfully.", ConsoleColor.DarkGreen);
            }
            catch (Exception error) {
                context.Output.Write("Failed to publish ", ConsoleColor.DarkRed)
                    .Write(definition.Id, ConsoleColor.Red)
                    .Write(" @", ConsoleColor.DarkRed)
                    .Write(version, ConsoleColor.Red)
                    .WriteLine("!", ConsoleColor.DarkRed)
                    .WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                throw;
            }
        }

        public async Task<ApplicationPackage> UnpackAsync(string filename, string path)
        {
            PathEx.CreatePath(path);

            ApplicationPackage metadata = null;

            await PackageTools.ReadArchive(filename, async archive => {
                metadata = await PackageTools.ParseMetadataAsync<ApplicationPackage>(archive);

                context.Output.Write("Unpacking ", ConsoleColor.DarkCyan)
                    .Write(metadata.Id, ConsoleColor.Cyan)
                    .Write(" @", ConsoleColor.DarkCyan)
                    .Write(metadata.Version, ConsoleColor.Cyan)
                    .WriteLine("...", ConsoleColor.DarkCyan);

                try {
                    await PackageTools.UnpackBin(archive, path);

                    context.Output.Write("Unpacked ", ConsoleColor.DarkBlue)
                        .Write(metadata.Id, ConsoleColor.Blue)
                        .Write(" @", ConsoleColor.DarkBlue)
                        .Write(metadata.Version, ConsoleColor.Blue)
                        .WriteLine(" successfully.", ConsoleColor.DarkBlue);
                }
                catch (Exception error) {
                    context.Output.Write("Failed to unpack ", ConsoleColor.DarkRed)
                        .Write(metadata.Id, ConsoleColor.Red)
                        .Write(" @", ConsoleColor.DarkRed)
                        .Write(metadata.Version, ConsoleColor.Red)
                        .WriteLine("!", ConsoleColor.DarkRed)
                        .WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                    throw;
                }
            });

            return metadata;
        }
    }
}
