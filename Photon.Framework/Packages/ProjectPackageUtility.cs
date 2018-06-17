using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using Photon.Framework.Tools;
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

            using (var block = context.Output.WriteBlock()) {
                block.Write("Parsing package definition ", ConsoleColor.DarkCyan);
                block.Write(name, ConsoleColor.Cyan);
                block.WriteLine("...", ConsoleColor.DarkCyan);
            }

            var definition = PackageTools.LoadDefinition<ProjectPackageDefinition>(definitionFilename);
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
                await ProjectPackageTools.CreatePackage(definition, rootPath, version, packageFilename);

                using (var block = context.Output.WriteBlock()) {
                    block.Write("Packaged ", ConsoleColor.DarkBlue);
                    block.Write(definition.Id, ConsoleColor.Blue);
                    block.Write(" @", ConsoleColor.DarkBlue);
                    block.Write(version, ConsoleColor.Blue);
                    block.WriteLine(" successfully.", ConsoleColor.DarkBlue);
                }
            }
            catch (Exception error) {
                using (var block = context.Output.WriteBlock()) {
                    block.Write("Failed to package ", ConsoleColor.DarkRed);
                    block.Write(definition.Id, ConsoleColor.Red);
                    block.Write(" @", ConsoleColor.DarkRed);
                    block.Write(version, ConsoleColor.Red);
                    block.WriteLine("!", ConsoleColor.DarkRed);
                    block.WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);
                }

                throw;
            }

            using (var block = context.Output.WriteBlock()) {
                block.Write("Publishing ", ConsoleColor.DarkCyan);
                block.Write(definition.Id, ConsoleColor.Cyan);
                block.Write(" @", ConsoleColor.DarkCyan);
                block.Write(version, ConsoleColor.Cyan);
                block.WriteLine("...", ConsoleColor.DarkCyan);
            }

            try {
                await context.PushProjectPackageAsync(packageFilename, token);

                using (var block = context.Output.WriteBlock()) {
                    block.Write("Published ", ConsoleColor.DarkGreen);
                    block.Write(definition.Id, ConsoleColor.Green);
                    block.Write(" @", ConsoleColor.DarkGreen);
                    block.Write(version, ConsoleColor.Green);
                    block.WriteLine(" successfully.", ConsoleColor.DarkGreen);
                }
            }
            catch (Exception error) {
                using (var block = context.Output.WriteBlock()) {
                    block.Write("Failed to publish ", ConsoleColor.DarkRed);
                    block.Write(definition.Id, ConsoleColor.Red);
                    block.Write(" @", ConsoleColor.DarkRed);
                    block.Write(version, ConsoleColor.Red);
                    block.WriteLine("!", ConsoleColor.DarkRed);
                    block.WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);
                }

                throw;
            }
        }

        public async Task<ProjectPackage> UnpackAsync(string filename, string path)
        {
            PathEx.CreatePath(path);

            ProjectPackage metadata = null;

            await PackageTools.ReadArchive(filename, async archive => {
                metadata = await PackageTools.ParseMetadataAsync<ProjectPackage>(archive);

                using (var block = context.Output.WriteBlock()) {
                    block.Write("Unpacking ", ConsoleColor.DarkCyan);
                    block.Write(metadata.Id, ConsoleColor.Cyan);
                    block.Write(" @", ConsoleColor.DarkCyan);
                    block.Write(metadata.Version, ConsoleColor.Cyan);
                    block.WriteLine("...", ConsoleColor.DarkCyan);
                }

                try {
                    await PackageTools.UnpackBin(archive, path);

                    using (var block = context.Output.WriteBlock()) {
                        block.Write("Unpacked ", ConsoleColor.DarkBlue);
                        block.Write(metadata.Id, ConsoleColor.Blue);
                        block.Write(" @", ConsoleColor.DarkBlue);
                        block.Write(metadata.Version, ConsoleColor.Blue);
                        block.WriteLine(" successfully.", ConsoleColor.DarkBlue);
                    }
                }
                catch (Exception error) {
                    using (var block = context.Output.WriteBlock()) {
                        block.Write("Failed to unpack ", ConsoleColor.DarkRed);
                        block.Write(metadata.Id, ConsoleColor.Red);
                        block.Write(" @", ConsoleColor.DarkRed);
                        block.Write(metadata.Version, ConsoleColor.Red);
                        block.WriteLine("!", ConsoleColor.DarkRed);
                        block.WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);
                    }

                    throw;
                }
            });

            return metadata;
        }
    }
}
