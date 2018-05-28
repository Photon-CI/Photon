﻿using Photon.Framework.Domain;
using Photon.Framework.Tools;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.NuGetPlugin
{
    public class NuGetPackagePublisher
    {
        private readonly IDomainContext context;

        public NugetModes Mode {get; set;}
        public NuGetCommandLine CL {get; set;}
        public NuGetCore Client {get; set;}

        public string PackageId {get; set;}
        public string Version {get; set;}
        public string PackageDirectory {get; set;}
        public string PackageDefinition {get; set;}
        public string Configuration {get; set;}
        public string Platform {get; set;}


        public NuGetPackagePublisher(IDomainContext context)
        {
            this.context = context;

            Configuration = "Release";
            Platform = "AnyCPU";
        }

        public async Task PublishAsync(CancellationToken token)
        {
            switch (Mode) {
                case NugetModes.Core:
                    if (!await PreCheckUsingCore(token)) return;
                    break;
                case NugetModes.CommandLine:
                    if (!PreCheckUsingCL()) return;
                    break;
            }

            switch (Mode) {
                case NugetModes.Core:
                    PackUsingCore();
                    break;
                case NugetModes.CommandLine:
                    PackUsingCL();
                    break;
            }

            //var packageFilename = Directory
            //    .GetFiles(PackageDirectory, $"{PackageId}.*.nupkg")
            //    .FirstOrDefault();

            //var packageName = Path.GetFileName(packageFilename);

            //context.Output?.Write("Publishing Package ", ConsoleColor.DarkCyan)
            //    .Write(packageName, ConsoleColor.Cyan)
            //    .WriteLine("...", ConsoleColor.DarkCyan);

            switch (Mode) {
                case NugetModes.Core:
                    await PushUsingCore(token);
                    break;
                case NugetModes.CommandLine:
                    PushUsingCL(token);
                    break;
            }
        }

        private async Task<bool> PreCheckUsingCore(CancellationToken token)
        {
            // Pre-Check Version
            context.Output.Write("Checking Package ", ConsoleColor.DarkCyan)
                .Write(PackageId, ConsoleColor.Cyan)
                .WriteLine(" for updates...", ConsoleColor.DarkCyan);

            var versionList = await Client.GetAllPackageVersions(PackageId, token);
            var packageVersion = versionList.Any() ? versionList.Max() : null;

            if (VersionTools.HasUpdates(packageVersion, Version)) return true;

            context.Output.Write($"Package '{PackageId}' is up-to-date. Version ", ConsoleColor.DarkBlue)
                .WriteLine(packageVersion, ConsoleColor.Blue);

            return false;
        }

        private void PackUsingCore()
        {
            var packageFilename = Path.Combine(PackageDirectory, $"{PackageId}.{Version}.nupkg");

            Client.Pack(PackageDefinition, packageFilename);
        }

        private async Task PushUsingCore(CancellationToken token)
        {
            var packageFilename = Path.Combine(PackageDirectory, $"{PackageId}.{Version}.nupkg");

            await Client.PushAsync(packageFilename, token);
        }

        private bool PreCheckUsingCL()
        {
            context.Output.WriteLine("Package version pre-check is not implemented in NuGet command-line mode!", ConsoleColor.DarkYellow);
            return true;
        }

        private void PackUsingCL()
        {
            // TODO: ?

            CL.Pack(PackageDefinition, PackageDirectory);
        }

        private void PushUsingCL(CancellationToken token)
        {
            var packageFilename = Directory
                .GetFiles(PackageDirectory, $"{PackageId}.*.nupkg")
                .FirstOrDefault();

            CL.Push(packageFilename, token);
        }
    }

    public enum NugetModes
    {
        Core,
        CommandLine,
    }
}
