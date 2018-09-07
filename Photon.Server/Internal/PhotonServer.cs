using log4net;
using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Packages;
using Photon.Library;
using Photon.Library.Packages;
using Photon.Library.Security;
using Photon.Library.Variables;
using Photon.Server.Internal.HealthChecks;
using Photon.Server.Internal.Http;
using Photon.Server.Internal.Packages;
using Photon.Server.Internal.Projects;
using Photon.Server.Internal.Security;
using Photon.Server.Internal.ServerAgents;
using Photon.Server.Internal.ServerConfiguration;
using Photon.Server.Internal.Sessions;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class PhotonServer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PhotonServer));
        public static PhotonServer Instance {get;} = new PhotonServer();

        private bool isStarted;

        public HttpServer Http {get;}
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
        public ProjectPackageCache ProjectPackageCache {get;}
        //public PackageCache ApplicationPackageCache {get;}


        public PhotonServer()
        {
            Http = new HttpServer(this);
            Projects = new ProjectManager();
            Sessions = new ServerSessionManager();
            MessageRegistry = new MessageProcessorRegistry();
            Variables = new VariableSetDocumentManager();
            HealthChecks = new HealthCheckService();
            UserMgr = new UserGroupManager();
            ProjectPackageCache = new ProjectPackageCache();
            ServerConfiguration = new ServerConfigurationManager();
            Agents = new ServerAgentManager();

            ProjectPackages = new ProjectPackageManager {
                PackageDirectory = Configuration.ProjectPackageDirectory,
            };

            ApplicationPackages = new ApplicationPackageManager {
                PackageDirectory = Configuration.ApplicationPackageDirectory,
            };

            Queue = new ScriptQueue {
                MaxDegreeOfParallelism = Configuration.Parallelism,
            };

            ProjectPackages.PackageAdded += ProjectPackages_OnPackageAdded;
        }

        public void Dispose()
        {
            if (isStarted) Stop();

            Queue?.Dispose();
            Sessions?.Dispose();
            HealthChecks.Dispose();
            Http.Dispose();
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

            Http.Initialize();

            MessageRegistry.Scan(Assembly.GetExecutingAssembly());
            MessageRegistry.Scan(typeof(ILibraryAssembly).Assembly);
            MessageRegistry.Scan(typeof(IFrameworkAssembly).Assembly);

            // TODO: Cache Project Package Index?
            //ProjectPackages.Initialize();

            Sessions.Start();
            Queue.Start();

            var taskVariables = Task.Run(() => Variables.Load(Configuration.VariablesDirectory));
            var taskHttp = Task.Run(() => Http.Start());
            var taskAgents = Task.Run(() => Agents.Load());
            var taskProjects = Task.Run(() => Projects.Load());

            Task.WaitAll(
                taskVariables,
                taskAgents,
                taskProjects,
                taskHttp);

            ProjectPackageCache.Initialize()
                .GetAwaiter().GetResult();

            HealthChecks.Start();

            Log.Info("Server started.");
        }

        public void Stop()
        {
            Log.Debug("Stopping Server...");

            Queue.Stop();
            Sessions.Stop();
            HealthChecks.Stop();

            Http.Stop();

            Log.Info("Server stopped.");
        }

        public async Task Shutdown(TimeSpan timeout)
        {
            Log.Debug("Shutdown started...");

            using (var tokenSource = new CancellationTokenSource(timeout)) {
                var token = tokenSource.Token;

                using (token.Register(() => {
                    Queue.Abort();
                    Sessions.Abort();
                })) {
                    await Task.Run(() => {
                        Queue.Stop();
                        Sessions.Stop();
                    }, token);
                }
            }
        }

        public void Abort()
        {
            Log.Debug("Server abort started...");

            Queue.Abort();
            Sessions.Abort();
            Http.Stop();
        }

        private void ProjectPackages_OnPackageAdded(IPackageMetadata metadata)
        {
            var projectMetadata = (ProjectPackage)metadata;
            ProjectPackageCache.AddPackage(projectMetadata);
        }
    }
}
