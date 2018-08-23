using log4net;
using Photon.Communication;
using Photon.Framework;
using Photon.Library;
using Photon.Library.Packages;
using Photon.Library.Security;
using Photon.Library.Variables;
using Photon.Server.Internal.HealthChecks;
using Photon.Server.Internal.Projects;
using Photon.Server.Internal.ServerAgents;
using Photon.Server.Internal.ServerConfiguration;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http;
using PiServerLite.Http.Content;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Photon.Library.Http.Security;
using Photon.Server.Internal.Security;

namespace Photon.Server.Internal
{
    internal class PhotonServer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PhotonServer));
        public static PhotonServer Instance {get;} = new PhotonServer();

        private HttpReceiver receiver;
        private bool isStarted;

        public HttpReceiverContext HttpContext {get; private set;}

        public ProjectManager Projects {get;}
        public ServerSessionManager Sessions {get;}
        public ScriptQueue Queue {get;}
        public ProjectPackageManager ProjectPackages {get;}
        public ApplicationPackageManager ApplicationPackages {get;}
        public MessageProcessorRegistry MessageRegistry {get;}
        public VariableSetDocumentManager Variables {get;}
        public ServerConfigurationManager ServerConfiguration {get;}
        public ServerAgentManager Agents {get;}
        public HealthCheckService HealthChecks {get;}
        public UserGroupManager UserMgr {get;}


        public PhotonServer()
        {
            Projects = new ProjectManager();
            Sessions = new ServerSessionManager();
            MessageRegistry = new MessageProcessorRegistry();
            Variables = new VariableSetDocumentManager();
            HealthChecks = new HealthCheckService();
            UserMgr = new UserGroupManager();

            ProjectPackages = new ProjectPackageManager {
                PackageDirectory = Configuration.ProjectPackageDirectory,
            };

            ApplicationPackages = new ApplicationPackageManager {
                PackageDirectory = Configuration.ApplicationPackageDirectory,
            };

            Queue = new ScriptQueue {
                MaxDegreeOfParallelism = Configuration.Parallelism,
            };

            ServerConfiguration = new ServerConfigurationManager();
            Agents = new ServerAgentManager();
        }

        public void Dispose()
        {
            if (isStarted) Stop();

            Queue?.Dispose();
            Sessions?.Dispose();
            HealthChecks.Dispose();
            receiver?.Dispose();
            receiver = null;
        }

        public void Start()
        {
            if (isStarted) throw new Exception("Server has already been started!");
            isStarted = true;

            Log.Debug("Starting Server...");

            // Load existing or default server configuration
            ServerConfiguration.Load();

            UserMgr.Directory = Configuration.Directory;
            
            if (!UserMgr.Initialize())
                DefaultSecurityGroups.Initialize(UserMgr);

            MessageRegistry.Scan(Assembly.GetExecutingAssembly());
            MessageRegistry.Scan(typeof(ILibraryAssembly).Assembly);
            MessageRegistry.Scan(typeof(IFrameworkAssembly).Assembly);

            // TODO: Cache Project Package Index?
            //ProjectPackages.Initialize();

            Sessions.Start();
            Queue.Start();

            var taskVariables = Task.Run(() => Variables.Load(Configuration.VariablesDirectory));
            var taskHttp = Task.Run(() => StartHttpServer());
            var taskAgents = Task.Run(() => Agents.Load());
            var taskProjects = Task.Run(() => Projects.Load());

            Task.WaitAll(
                taskVariables,
                taskAgents,
                taskProjects,
                taskHttp);

            HealthChecks.Start();

            Log.Info("Server started.");
        }

        public void Stop()
        {
            Log.Debug("Stopping Server...");

            Queue.Stop();
            Sessions.Stop();
            HealthChecks.Stop();

            try {
                receiver?.Dispose();
                receiver = null;
            }
            catch (Exception error) {
                Log.Error("Failed to stop HTTP Receiver!", error);
            }

            Log.Info("Server stopped.");
        }

        public async Task Shutdown(TimeSpan timeout)
        {
            Log.Debug("Shutdown started...");

            using (var tokenSource = new CancellationTokenSource(timeout)) {
                var token = tokenSource.Token;

                token.Register(() => {
                    Queue.Abort();
                    Sessions.Abort();
                });

                await Task.Run(() => {
                    Queue.Stop();
                    Sessions.Stop();
                }, token);
            }
        }

        public void Abort()
        {
            Log.Debug("Server abort started...");

            Queue.Abort();
            Sessions.Abort();

            try {
                receiver?.Dispose();
                receiver = null;
            }
            catch (Exception error) {
                Log.Error("Failed to stop HTTP Receiver!", error);
            }
        }

        private void StartHttpServer()
        {
            var config = ServerConfiguration.Value;
            var http = ServerConfiguration.Value.Http;

            var contentDir = new ContentDirectory {
                DirectoryPath = Configuration.HttpContentDirectory,
                UrlPath = "/Content/",
            };

            var sharedContentDir = new ContentDirectory {
                DirectoryPath = Configuration.HttpSharedContentDirectory,
                UrlPath = "/SharedContent/",
            };

            HttpContext = new HttpReceiverContext {
                ListenerPath = http.Path,
                SecurityMgr = new HttpSecurityManager {
                    Authorization = new HybridAuthorization {
                        UserMgr = UserMgr,
                        DomainEnabled = config.Security?.DomainEnabled ?? false,
                    },
                    Restricted = config.Security?.Enabled ?? false,
                    CookieName = "PHOTON.SERVER.AUTH",
                },
                ContentDirectories = {
                    contentDir,
                    sharedContentDir,
                },
            };

            HttpContext.Views.AddFolderFromExternal(Configuration.HttpViewDirectory);

            var httpPrefix = $"http://{http.Host}:{http.Port}/";

            if (!string.IsNullOrEmpty(http.Path))
                httpPrefix = NetPath.Combine(httpPrefix, http.Path);

            if (!httpPrefix.EndsWith("/"))
                httpPrefix += "/";

            receiver = new HttpReceiver(HttpContext);
            receiver.Routes.Scan(Assembly.GetExecutingAssembly());
            receiver.AddPrefix(httpPrefix);

            try {
                receiver.Start();

                Log.Debug($"HTTP Server listening at {httpPrefix}");
            }
            catch (Exception error) {
                Log.Error("Failed to start HTTP Receiver!", error);
            }
        }
    }
}
