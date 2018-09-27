using log4net;
using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Packages;
using Photon.Library;
using Photon.Library.Security;
using Photon.Library.Variables;
using Photon.Server.Internal.HealthChecks;
using Photon.Server.Internal.Http;
using Photon.Server.Internal.Packages;
using Photon.Server.Internal.Security;
using Photon.Server.Internal.ServerAgents;
using Photon.Server.Internal.ServerConfiguration;
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
        public MessageProcessorRegistry MessageRegistry {get;}
        public VariableSetDocumentManager Variables {get;}
        public ServerConfigurationManager ServerConfiguration {get;}
        public ServerAgentManager Agents {get;}
        public HealthCheckService HealthChecks {get;}
        public UserGroupManager UserMgr {get;}
        public ProjectPackageCache ProjectPackageCache {get;}

        public ServerContext Context {get; set;}


        public PhotonServer()
        {
            Http = new HttpServer(this);
            MessageRegistry = new MessageProcessorRegistry();
            Variables = new VariableSetDocumentManager();
            HealthChecks = new HealthCheckService();
            UserMgr = new UserGroupManager();
            ServerConfiguration = new ServerConfigurationManager();
            Agents = new ServerAgentManager();

            Context = new ServerContext();

            ProjectPackageCache = new ProjectPackageCache(Context.ProjectPackages);

            Context.ProjectPackages.PackageAdded += ProjectPackages_OnPackageAdded;
        }

        public void Dispose()
        {
            if (isStarted) Stop();

            Context?.Dispose();
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

            Context.Initialize();

            var taskVariables = Task.Run(() => Variables.Load(Configuration.VariablesDirectory));
            var taskHttp = Task.Run(() => Http.Start());
            var taskAgents = Task.Run(() => Agents.Load());
            var taskProjects = Task.Run(() => Context.Projects.Load());

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

            Context.End();
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
                    Context.Queue.Abort();
                    Context.Sessions.Abort();
                })) {
                    await Task.Run(() => {
                        Context.End();
                    }, token);
                }
            }
        }

        public void Abort()
        {
            Log.Debug("Server abort started...");

            Context.Queue.Abort();
            Context.Sessions.Abort();
            Http.Stop();
        }

        private void ProjectPackages_OnPackageAdded(IPackageMetadata metadata)
        {
            var projectMetadata = (ProjectPackage)metadata;
            ProjectPackageCache.AddPackage(projectMetadata);
        }
    }
}
