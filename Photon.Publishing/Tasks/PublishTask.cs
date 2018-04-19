using Newtonsoft.Json;
using Photon.Framework;
using Photon.Framework.Agent;
using Photon.Framework.Tasks;
using Photon.Framework.Tools;
using Photon.Publishing.Internal;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing.Tasks
{
    public class PublishTask : IBuildTask
    {
        private string nugetPackageDir;
        //private string downloadUrl;
        private string apiUrl;
        private string ftpUrl;
        private string ftpUser;
        private string ftpPass;

        public IAgentBuildContext Context {get; set;}


        public async Task RunAsync(CancellationToken token)
        {
            var photonVars = Context.ServerVariables["photon"];

            if (photonVars == null)
                throw new ApplicationException("Photon Variables were not found!");

            nugetPackageDir = Path.Combine(Context.WorkDirectory, "Packages");
            //downloadUrl = "http://download.photon.null511.info";
            apiUrl = "http://photon.null511.info/api";
            ftpUrl = photonVars["ftp.url"];
            ftpUser = photonVars["ftp.user"];
            ftpPass = photonVars["ftp.pass"];

            await BuildSolution();
            await PublishFramework(token);
            await PublishServer();
            await PublishAgent();
            await PublishCLI();
        }

        private async Task BuildSolution()
        {
            await Context.RunCommandLineAsync(
                ".\\bin\\msbuild.cmd", "/m", "/v:m",
                "Photon.sln",
                "/p:Configuration=Release",
                "/p:Platform=\"Any CPU\"",
                "/t:Rebuild");
        }

        private async Task PublishFramework(CancellationToken token)
        {
            var assemblyFilename = Path.Combine(Context.ContentDirectory, "Photon.Framework", "bin", "Release", "Photon.Framework.dll");

            var publisher = new NugetPackagePublisher(Context) {
                ProjectFile = Path.Combine("Photon.Framework", "Photon.Framework.csproj"),
                AssemblyVersion = AssemblyTools.GetVersion(assemblyFilename),
                PackageId = "photon.framework",
                PackageDirectory = nugetPackageDir,
            };

            await publisher.PublishAsync(token);
        }

        private async Task PublishServer()
        {
            var versionUrl = NetPath.Combine(apiUrl, "server/version");
            var assemblyFilename = Path.Combine(Context.ContentDirectory, "Photon.Server", "bin", "Release", "PhotonServer.exe");
            var msiFilename = Path.Combine(Context.ContentDirectory, "Installers", "Photon.Server.Installer", "bin", "Release", "Photon.Server.Installer.msi");
            await PublishApplication("server", "Photon Server", "Photon.Server", assemblyFilename, msiFilename, versionUrl);
        }

        private async Task PublishAgent()
        {
            var versionUrl = NetPath.Combine(apiUrl, "agent/version");
            var assemblyFilename = Path.Combine(Context.ContentDirectory, "Photon.Agent", "bin", "Release", "PhotonAgent.exe");
            var msiFilename = Path.Combine(Context.ContentDirectory, "Installers", "Photon.Agent.Installer", "bin", "Release", "Photon.Agent.Installer.msi");
            await PublishApplication("agent", "Photon Agent", "Photon.Agent", assemblyFilename, msiFilename, versionUrl);
        }

        private async Task PublishCLI()
        {
            var versionUrl = NetPath.Combine(apiUrl, "cli/version");
            var assemblyFilename = Path.Combine(Context.ContentDirectory, "Photon.CLI", "bin", "Release", "PhotonCLI.exe");
            var msiFilename = Path.Combine(Context.ContentDirectory, "Installers", "Photon.CLI.Installer", "bin", "Release", "Photon.CLI.Installer.msi");
            await PublishApplication("cli", "Photon CLI", "Photon.CLI", assemblyFilename, msiFilename, versionUrl);
        }

        private async Task PublishApplication(string path, string appName, string fileName, string assemblyFilename, string msiFilename, string versionUrl)
        {
            var assemblyVersion = AssemblyTools.GetVersion(assemblyFilename);

            string _version;
            using (var webClient = new WebClient()) {
                var response = await webClient.DownloadStringTaskAsync(versionUrl);
                _version = response.Trim();
            }

            if (!string.IsNullOrEmpty(_version)) {
                var segmentCount = _version.Split('.').Length;

                for (var i = segmentCount; i < 4; i++) {
                    _version += ".0";
                }

                var webVersion = new Version(_version);

                if (webVersion >= assemblyVersion) {
                    Context.Output
                        .Append($"{appName} is up-to-date. Version ", ConsoleColor.DarkYellow)
                        .AppendLine(webVersion, ConsoleColor.Yellow);

                    return;
                }
            }

            // Publish

            var url = NetPath.Combine(ftpUrl, $"{path}/{assemblyVersion}");
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;

            if (!string.IsNullOrEmpty(ftpUser) || !string.IsNullOrEmpty(ftpPass))
                request.Credentials = new NetworkCredential (ftpUser, ftpPass);

            using (var _ = (FtpWebResponse)await request.GetResponseAsync()) {
                //
            }

            var webMsiName = $"{fileName}.{assemblyVersion}.msi";
            var webZipName = $"{fileName}.{assemblyVersion}.zip";
            var msiWebPath = NetPath.Combine(path, assemblyVersion.ToString(), webMsiName);

            await UploadFile(msiFilename, msiWebPath);

            // TODO: Create and Upload ZIP

            var index = new {
                version = assemblyVersion.ToString(),
                msiFilename = webMsiName,
                zipFilename = webZipName,
                notes = "...",
            };

            await UploadIndex(index, path, assemblyVersion.ToString());
        }

        private async Task UploadFile(string filename, string webPath)
        {
            var msiUrl = NetPath.Combine(ftpUrl, webPath);

            var request = (FtpWebRequest)WebRequest.Create(msiUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            if (!string.IsNullOrEmpty(ftpUser) || !string.IsNullOrEmpty(ftpPass))
                request.Credentials = new NetworkCredential (ftpUser, ftpPass);

            using (var fileStream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                request.ContentLength = fileStream.Length;

                using (var requestStream = await request.GetRequestStreamAsync()) {
                    await fileStream.CopyToAsync(requestStream);
                }
            }

            using (var _ = (FtpWebResponse) await request.GetResponseAsync()) {
                //...
            }
        }

        private async Task UploadIndex(object index, string appName, string version)
        {
            var url = NetPath.Combine(ftpUrl, $"{appName}/{version}/index.json");
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            if (!string.IsNullOrEmpty(ftpUser) || !string.IsNullOrEmpty(ftpPass))
                request.Credentials = new NetworkCredential (ftpUser, ftpPass);

            using (var stream = await request.GetRequestStreamAsync())
            using (var writer = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(writer)) {
                JsonSettings.Serializer.Serialize(jsonWriter, index);
            }

            using (var _ = (FtpWebResponse)await request.GetResponseAsync()) {
                //...
            }
        }
    }
}
