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

            using (var block = context.Output.WriteBlock()) {
                block.Write("Parsing package definition ", ConsoleColor.DarkCyan);
                block.Write(name, ConsoleColor.Cyan);
                block.WriteLine("...", ConsoleColor.DarkCyan);
            }

            var definition = PackageTools.LoadDefinition<ApplicationPackageDefinition>(definitionFilename);
            var rootPath = Path.GetDirectoryName(definitionFilename);
            var packageFilename = Path.Combine(PackageDirectory, $"{definition.Id}.zip");

            using (var block = context.Output.WriteBlock()) {
                block.Write("Packaging ", ConsoleColor.DarkCyan);
                block.Write(definition.Id, ConsoleColor.Cyan);
                block.Write(" @", ConsoleColor.DarkCyan);
                block.Write(version, ConsoleColor.Cyan);
                block.WriteLine("...", ConsoleColor.DarkCyan);
            }

            try {
                await ApplicationPackageTools.CreatePackage(definition, rootPath, version, packageFilename);

                using (var block = context.Output.WriteBlock()) {
                    block.Write("Packaged ", ConsoleColor.DarkBlue);
                    block.Write(definition.Id, ConsoleColor.Blue);
                    block.Write(" @", ConsoleColor.DarkBlue);
                    block.Write(version, ConsoleColor.Blue);
                    block.WriteLine(" successfully.", ConsoleColor.DarkBlue);
                }
            }
            catch (Exception error) {
                context.Output.WriteBlock()
                    .Write("Failed to package ", ConsoleColor.DarkRed)
                    .Write(definition.Id, ConsoleColor.Red)
                    .Write(" @", ConsoleColor.DarkRed)
                    .Write(version, ConsoleColor.Red)
                    .WriteLine("!", ConsoleColor.DarkRed)
                    .WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow)
                    .Post();

                throw;
            }

            context.Output.WriteBlock()
                .Write("Publishing ", ConsoleColor.DarkCyan)
                .Write(definition.Id, ConsoleColor.Cyan)
                .Write(" @", ConsoleColor.DarkCyan)
                .Write(version, ConsoleColor.Cyan)
                .WriteLine("...", ConsoleColor.DarkCyan)
                .Post();

            try {
                await context.PushApplicationPackageAsync(packageFilename, token);

                context.Output.WriteBlock()
                    .Write("Published ", ConsoleColor.DarkGreen)
                    .Write(definition.Id, ConsoleColor.Green)
                    .Write(" @", ConsoleColor.DarkGreen)
                    .Write(version, ConsoleColor.Green)
                    .WriteLine(" successfully.", ConsoleColor.DarkGreen)
                    .Post();
            }
            catch (Exception error) {
                context.Output.WriteBlock()
                    .Write("Failed to publish ", ConsoleColor.DarkRed)
                    .Write(definition.Id, ConsoleColor.Red)
                    .Write(" @", ConsoleColor.DarkRed)
                    .Write(version, ConsoleColor.Red)
                    .WriteLine("!", ConsoleColor.DarkRed)
                    .WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow)
                    .Post();

                throw;
            }
        }

        public async Task<ApplicationPackage> UnpackAsync(string filename, string path)
        {
            PathEx.CreatePath(path);

            ApplicationPackage metadata = null;

            await PackageTools.ReadArchive(filename, async archive => {
                metadata = await PackageTools.ParseMetadataAsync<ApplicationPackage>(archive);

                context.Output.WriteBlock()
                    .Write("Unpacking ", ConsoleColor.DarkCyan)
                    .Write(metadata.Id, ConsoleColor.Cyan)
                    .Write(" @", ConsoleColor.DarkCyan)
                    .Write(metadata.Version, ConsoleColor.Cyan)
                    .WriteLine("...", ConsoleColor.DarkCyan)
                    .Post();

                try {
                    await PackageTools.UnpackBin(archive, path);

                    context.Output.WriteBlock()
                        .Write("Unpacked ", ConsoleColor.DarkBlue)
                        .Write(metadata.Id, ConsoleColor.Blue)
                        .Write(" @", ConsoleColor.DarkBlue)
                        .Write(metadata.Version, ConsoleColor.Blue)
                        .WriteLine(" successfully.", ConsoleColor.DarkBlue)
                        .Post();
                }
                catch (Exception error) {
                    context.Output.WriteBlock()
                        .Write("Failed to unpack ", ConsoleColor.DarkRed)
                        .Write(metadata.Id, ConsoleColor.Red)
                        .Write(" @", ConsoleColor.DarkRed)
                        .Write(metadata.Version, ConsoleColor.Red)
                        .WriteLine("!", ConsoleColor.DarkRed)
                        .WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow)
                        .Post();

                    throw;
                }
            });

            return metadata;
        }
    }
}
