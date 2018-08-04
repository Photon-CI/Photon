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
            context.Output.WriteLine($"Parsing package definition '{name}'...", ConsoleColor.Gray);

            var definition = PackageTools.LoadDefinition<ApplicationPackageDefinition>(definitionFilename);
            var rootPath = Path.GetDirectoryName(definitionFilename);
            var packageFilename = Path.Combine(PackageDirectory, $"{definition.Id}.zip");

            var packageName = $"{definition.Id} @ {version}";
            context.Output.WriteLine($"Packaging '{packageName}'...", ConsoleColor.Gray);

            try {
                await ApplicationPackageTools.CreatePackage(definition, rootPath, version, packageFilename);

                context.Output.WriteLine($"Packaged '{packageName}' successfully.", ConsoleColor.Gray);
            }
            catch (Exception error) {
                using (var block = context.Output.WriteBlock()) {
                    block.Write($"Failed to package '{packageName}'! ", ConsoleColor.DarkRed);
                    block.WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);
                }

                throw;
            }

            context.Output.WriteLine($"Publishing '{packageName}'...", ConsoleColor.Gray);

            try {
                await context.PushApplicationPackageAsync(packageFilename, token);

                context.Output.WriteLine($"Published '{packageName}' successfully.", ConsoleColor.DarkCyan);
            }
            catch (Exception error) {
                using (var block = context.Output.WriteBlock()) {
                    block.Write($"Failed to publish '{packageName}'! ", ConsoleColor.DarkRed);
                    block.WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);
                }

                throw;
            }
        }

        public async Task<ApplicationPackage> UnpackAsync(string filename, string path)
        {
            PathEx.CreatePath(path);

            ApplicationPackage metadata = null;

            await PackageTools.ReadArchive(filename, async archive => {
                metadata = await PackageTools.ParseMetadataAsync<ApplicationPackage>(archive);

                var packageName = $"{metadata.Id} @ {metadata.Version}";

                context.Output.WriteLine($"Unpacking '{packageName}'...", ConsoleColor.Gray);

                try {
                    await PackageTools.UnpackBin(archive, path);

                    context.Output.WriteLine($"Unpacked '{packageName}' successfully.", ConsoleColor.Gray);
                }
                catch (Exception error) {
                    using (var block = context.Output.WriteBlock()) {
                        block.Write($"Failed to unpack '{packageName}'! ", ConsoleColor.DarkRed);
                        block.WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);
                    }

                    throw;
                }
            });

            return metadata;
        }
    }
}
