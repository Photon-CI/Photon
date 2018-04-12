using log4net;
using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Library;
using Photon.Library.Packages;
using Photon.Server.Internal.Projects;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http;
using PiServerLite.Http.Content;
using System;
using System.IO;
using System.Reflection;

namespace Photon.Server.Internal
{
    internal class PhotonServer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PhotonServer));
        public static PhotonServer Instance {get;} = new PhotonServer();

        private HttpReceiver receiver;
        private bool isStarted;

        public ServerDefinition Definition {get; private set;}
        public ProjectManager Projects {get;}
        public ServerSessionManager Sessions {get;}
        public ProjectDataManager ProjectData {get;}
        public ScriptQueue Queue {get;}
        public string WorkPath {get;}
        public ProjectPackageManager ProjectPackages {get;}
        public ApplicationPackageManager ApplicationPackages {get;}
        public MessageProcessorRegistry MessageRegistry {get;}


        public PhotonServer()
        {
            Projects = new ProjectManager();
            Sessions = new ServerSessionManager();
            ProjectData = new ProjectDataManager();
            MessageRegistry = new MessageProcessorRegistry();

            ProjectPackages = new ProjectPackageManager {
                PackageDirectory = Configuration.ProjectPackageDirectory,
            };

            ApplicationPackages = new ApplicationPackageManager {
                PackageDirectory = Configuration.ApplicationPackageDirectory,
            };

            Queue = new ScriptQueue {
                MaxDegreeOfParallelism = Configuration.Parallelism,
            };

            WorkPath = Configuration.WorkDirectory;
        }

        public void Dispose()
        {
            if (isStarted) Stop();

            Sessions?.Dispose();
            receiver?.Dispose();
            receiver = null;
        }

        public void Start()
        {
            isStarted = true;

            // Load existing or default server configuration
            Definition = ParseServerDefinition() ?? new ServerDefinition {
                Http = {
                    Host = "localhost",
                    Port = 8088,
                    Path = "/photon/server",
                },
            };

            Projects.Initialize();
            ProjectData.Initialize();

            MessageRegistry.Scan(Assembly.GetExecutingAssembly());
            MessageRegistry.Scan(typeof(ILibraryAssembly).Assembly);
            MessageRegistry.Scan(typeof(IFrameworkAssembly).Assembly);

            // TODO: Cache Project Package Index?
            //ProjectPackages.Initialize();

            Sessions.Start();
            Queue.Start();

            StartHttpServer();
        }

        public void Stop()
        {
            Queue.Stop();
            Sessions.Stop();

            try {
                receiver?.Dispose();
                receiver = null;
            }
            catch (Exception error) {
                Log.Error("Failed to stop HTTP Receiver!", error);
            }
        }

        private ServerDefinition ParseServerDefinition()
        {
            var file = Configuration.ServerFile ?? "server.json";
            var path = Path.Combine(Configuration.AssemblyPath, file);
            path = Path.GetFullPath(path);

            Log.Debug($"Loading Server Definition: {path}");

            if (!File.Exists(path)) {
                Log.Warn($"Server Definition not found! {path}");
                return null;
            }

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                return JsonSettings.Serializer.Deserialize<ServerDefinition>(stream);
            }
        }

        private void StartHttpServer()
        {
            var context = new HttpReceiverContext {
                //SecurityMgr = new Internal.Security.SecurityManager(),
                ListenerPath = Definition.Http.Path,
                ContentDirectories = {
                    new ContentDirectory {
                        DirectoryPath = Path.Combine(Configuration.AssemblyPath, "Content"),
                        UrlPath = "/Content/",
                    }
                },
            };

            var viewPath = Path.Combine(Configuration.AssemblyPath, "Views");
            context.Views.AddFolderFromExternal(viewPath);

            var httpPrefix = $"http://{Definition.Http.Host}:{Definition.Http.Port}/";

            if (!string.IsNullOrEmpty(Definition.Http.Path))
                httpPrefix = NetPath.Combine(httpPrefix, Definition.Http.Path);

            if (!httpPrefix.EndsWith("/"))
                httpPrefix += "/";

            receiver = new HttpReceiver(context);
            receiver.Routes.Scan(Assembly.GetExecutingAssembly());
            receiver.AddPrefix(httpPrefix);

            try {
                receiver.Start();

                Log.Info($"HTTP Server listening at http://{httpPrefix}");
            }
            catch (Exception error) {
                Log.Error("Failed to start HTTP Receiver!", error);
            }
        }
    }
}
