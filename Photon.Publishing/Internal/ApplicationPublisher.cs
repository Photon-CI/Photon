using Newtonsoft.Json;
using Photon.Framework;
using Photon.Framework.Domain;
using Photon.Framework.Tools;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing.Internal
{
    internal class ApplicationPublisher
    {
        private readonly IDomainContext context;

        public string FtpUsername {get; set;}
        public string FtpPassword {get; set;}
        public string VersionUrl {get; set;}
        public string UploadPath {get; set;}
        public string AssemblyFilename {get; set;}
        public string MsiFilename {get; set;}
        public string PackagePath {get; set;}
        public string BinPath {get; set;}


        public ApplicationPublisher(IDomainContext context)
        {
            this.context = context;
        }

        public async Task PublishAsync(string packageName, string packageId, CancellationToken token = default)
        {
            using (var block = context.Output.WriteBlock()) {
                block.Write("Updating Application ", ConsoleColor.DarkCyan);
                block.Write(packageName, ConsoleColor.Cyan);
                block.WriteLine("...", ConsoleColor.DarkCyan);
            }

            var photonVars = context.ServerVariables["photon"];

            if (photonVars == null)
                throw new ApplicationException("Photon Variables were not found!");

            var assemblyVersion = AssemblyTools.GetVersion(AssemblyFilename);

            var webVersion = await GetWebVersion();

            if (!VersionTools.HasUpdates(webVersion, assemblyVersion)) {
                using (var block = context.Output.WriteBlock()) {
                    block.Write("Application ", ConsoleColor.DarkBlue);
                    block.Write(packageName, ConsoleColor.Blue);
                    block.Write(" is up-to-date. Version ", ConsoleColor.DarkBlue);
                    block.WriteLine(assemblyVersion, ConsoleColor.Blue);
                }

                return;
            }

            // Publish

            // Create ZIP
            PathEx.CreatePath(PackagePath);

            var zipFilename = Path.Combine(PackagePath, $"{packageId}.zip");
            await CreateZip(BinPath, zipFilename);

            // Create Version Directory
            try {
                await CreateWebPath(assemblyVersion, token);
            }
            catch (WebException) {
                //...
            }

            var webMsiName = $"{packageId}.{assemblyVersion}.msi";
            var webZipName = $"{packageId}.{assemblyVersion}.zip";
            var msiWebUrl = NetPath.Combine(UploadPath, assemblyVersion, webMsiName);
            var zipWebUrl = NetPath.Combine(UploadPath, assemblyVersion, webZipName);

            await UploadFile(MsiFilename, msiWebUrl, token);
            await UploadFile(zipFilename, zipWebUrl, token);

            var index = new {
                version = assemblyVersion,
                msiFilename = webMsiName,
                zipFilename = webZipName,
                notes = "...",
            };

            await UploadIndex(index, assemblyVersion, token);

            await UpdateLatest(assemblyVersion, token);

            using (var block = context.Output.WriteBlock()) {
                block.Write("Application ", ConsoleColor.DarkGreen);
                block.Write(packageName, ConsoleColor.Green);
                block.Write(" updated successfully. ", ConsoleColor.DarkGreen);
                block.Write("Version ", ConsoleColor.DarkCyan);
                block.WriteLine(assemblyVersion, ConsoleColor.Cyan);
            }
        }

        private async Task CreateWebPath(string assemblyVersion, CancellationToken token)
        {
            var url = NetPath.Combine(UploadPath, assemblyVersion);
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;

            if (!string.IsNullOrEmpty(FtpUsername) || !string.IsNullOrEmpty(FtpPassword))
                request.Credentials = new NetworkCredential (FtpUsername, FtpPassword);

            using (token.Register(() => request.Abort()))
            using (await request.GetResponseAsync()) {}
        }

        private async Task<string> GetWebVersion()
        {
            using (var webClient = new WebClient()) {
                return (await webClient.DownloadStringTaskAsync(VersionUrl)).Trim();
            }
        }

        private async Task UploadFile(string filename, string url, CancellationToken token)
        {
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            if (!string.IsNullOrEmpty(FtpUsername) || !string.IsNullOrEmpty(FtpPassword))
                request.Credentials = new NetworkCredential(FtpUsername, FtpPassword);

            using (token.Register(() => request.Abort())) {
                using (var fileStream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                    request.ContentLength = fileStream.Length;

                    using (var requestStream = await request.GetRequestStreamAsync()) {
                        await fileStream.CopyToAsync(requestStream);
                    }
                }

                using (await request.GetResponseAsync()) {}
            }
        }

        private async Task UploadIndex(object index, string version, CancellationToken token)
        {
            var url = NetPath.Combine(UploadPath, $"{version}/index.json");
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            if (!string.IsNullOrEmpty(FtpUsername) || !string.IsNullOrEmpty(FtpPassword))
                request.Credentials = new NetworkCredential (FtpUsername, FtpPassword);

            using (token.Register(() => request.Abort())) {
                using (var stream = await request.GetRequestStreamAsync())
                using (var writer = new StreamWriter(stream))
                using (var jsonWriter = new JsonTextWriter(writer)) {
                    JsonSettings.Serializer.Serialize(jsonWriter, index);
                }

                using (await request.GetResponseAsync()) {}
            }
        }

        private async Task UpdateLatest(string version, CancellationToken token)
        {
            var url = NetPath.Combine(UploadPath, ".version");
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.DeleteFile;

            if (!string.IsNullOrEmpty(FtpUsername) || !string.IsNullOrEmpty(FtpPassword))
                request.Credentials = new NetworkCredential (FtpUsername, FtpPassword);

            using (token.Register(() => request.Abort()))
            using (await request.GetResponseAsync()) {}

            //------------------

            var request2 = (FtpWebRequest)WebRequest.Create(url);
            request2.Method = WebRequestMethods.Ftp.UploadFile;

            if (!string.IsNullOrEmpty(FtpUsername) || !string.IsNullOrEmpty(FtpPassword))
                request2.Credentials = new NetworkCredential (FtpUsername, FtpPassword);

            using (token.Register(() => request.Abort())) {
                using (var stream = await request2.GetRequestStreamAsync())
                using (var writer = new StreamWriter(stream)) {
                    writer.Write(version);
                }

                using (await request2.GetResponseAsync()) {}
            }
        }

        private static async Task CreateZip(string sourcePath, string filename)
        {
            var path_abs = Path.GetFullPath(sourcePath);

            using (var stream = File.Open(filename, FileMode.Create, FileAccess.Write))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create)) {
                foreach (var file in Directory.EnumerateFiles(path_abs, "*.*", SearchOption.AllDirectories)) {
                    var localName = file;

                    if (localName.StartsWith(path_abs))
                        localName = localName.Substring(path_abs.Length);

                    if (localName.StartsWith(Path.DirectorySeparatorChar.ToString()))
                        localName = localName.Substring(1);

                    var entry = archive.CreateEntry(localName);

                    using (var fileStream = File.Open(file, FileMode.Open, FileAccess.Read))
                    using (var entryStream = entry.Open()) {
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }
        }
    }
}
