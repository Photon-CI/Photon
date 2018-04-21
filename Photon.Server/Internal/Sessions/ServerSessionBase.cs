using log4net;
using Photon.Framework;
using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using Photon.Framework.Packages;
using Photon.Framework.Server;
using Photon.Framework.Tasks;
using Photon.Framework.Variables;
using Photon.Library;
using Photon.Library.Packages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal abstract class ServerSessionBase : IServerSession
    {
        private readonly ConcurrentDictionary<string, DomainAgentSessionHostBase> hostList;
        internal readonly List<PackageReference> PushedProjectPackageList;

        private readonly Lazy<ILog> _log;
        private readonly DateTime utcCreated;
        private DateTime? utcReleased;
        private bool isReleased;

        public string SessionId {get;}
        public string WorkDirectory {get;}
        public string BinDirectory {get;}
        public string ContentDirectory {get;}
        protected ServerDomain Domain {get; set;}
        public bool IsComplete {get; set;}
        public TimeSpan CacheSpan {get; set;}
        public TimeSpan LifeSpan {get; set;}
        public Exception Exception {get; set;}
        public ScriptOutput Output {get;}
        protected DomainConnectionFactory ConnectionFactory {get;}
        protected DomainPackageClient PackageClient {get;}
        public TaskResult Result {get; private set;}
        public VariableSetCollection Variables {get;}
        protected ILog Log => _log.Value;

        protected readonly CancellationTokenSource TokenSource;
        private readonly ProjectPackageManager projectPackages;
        private readonly ApplicationPackageManager applicationPackages;

        public IEnumerable<PackageReference> PushedProjectPackages => PushedProjectPackageList;


        protected ServerSessionBase()
        {
            SessionId = Guid.NewGuid().ToString("N");
            utcCreated = DateTime.UtcNow;
            CacheSpan = TimeSpan.FromHours(1);
            LifeSpan = TimeSpan.FromHours(8);
            Output = new ScriptOutput();

            _log = new Lazy<ILog>(() => LogManager.GetLogger(GetType()));
            hostList = new ConcurrentDictionary<string, DomainAgentSessionHostBase>(StringComparer.Ordinal);
            PushedProjectPackageList = new List<PackageReference>();
            WorkDirectory = Path.Combine(Configuration.WorkDirectory, SessionId);
            BinDirectory = Path.Combine(WorkDirectory, "bin");
            ContentDirectory = Path.Combine(WorkDirectory, "content");

            projectPackages = new ProjectPackageManager {
                PackageDirectory = Configuration.ProjectPackageDirectory,
            };

            applicationPackages = new ApplicationPackageManager {
                PackageDirectory = Configuration.ApplicationPackageDirectory,
            };

            Variables = PhotonServer.Instance.Variables;

            ConnectionFactory = new DomainConnectionFactory();
            ConnectionFactory.OnConnectionRequest += ConnectionFactory_OnConnectionRequest;

            PackageClient = new DomainPackageClient();
            PackageClient.OnPushProjectPackage += PackageClient_OnPushProjectPackage;
            PackageClient.OnPushApplicationPackage += PackageClient_OnPushApplicationPackage;
            PackageClient.OnPullProjectPackage += PackageClient_OnPullProjectPackage;
            PackageClient.OnPullApplicationPackage += PackageClient_OnPullApplicationPackage;

            TokenSource = new CancellationTokenSource();
        }

        public virtual void Dispose()
        {
            if (!isReleased)
                ReleaseAsync().GetAwaiter().GetResult();

            TokenSource?.Dispose();
            Domain?.Dispose();
            Domain = null;
        }

        protected abstract DomainAgentSessionHostBase OnCreateHost(ServerAgentDefinition agent);

        public virtual async Task PrepareWorkDirectoryAsync()
        {
            await Task.Run(() => {
                Directory.CreateDirectory(WorkDirectory);
                Directory.CreateDirectory(BinDirectory);
                Directory.CreateDirectory(ContentDirectory);
            });
        }

        public abstract Task RunAsync();

        public async Task ReleaseAsync()
        {
            if (isReleased) return;
            isReleased = true;
            utcReleased = DateTime.UtcNow;

            foreach (var host in hostList.Values) {
                try {
                    host.Dispose();
                }
                catch (Exception error) {
                    Log.Error("Failed to close host!", error);
                }
            }
            hostList.Clear();

            if (Domain != null) {
                try {
                    await Domain.Unload(true);
                    Domain = null;
                }
                catch (Exception error) {
                    Log.Error("Failed to unload domain!", error);
                }
            }

            var workDirectory = WorkDirectory;
            try {
                await Task.Run(() => FileUtils.DestoryDirectory(workDirectory));
            }
            catch (AggregateException errors) {
                errors.Flatten().Handle(e => {
                    if (e is IOException ioError) {
                        Log.Warn(ioError.Message);
                        return true;
                    }

                    return false;
                });
            }
        }

        public void Abort()
        {
            TokenSource.Cancel();

            //foreach (var host in hostList.Values)
            //    host.Abort();

            // TODO: Wait?
        }

        public void Complete(TaskResult result)
        {
            this.Result = result;

            Output.Flush();
            IsComplete = true;
        }

        public bool IsExpired()
        {
            if (utcReleased.HasValue) {
                if (DateTime.UtcNow - utcReleased > CacheSpan)
                    return true;
            }

            return DateTime.UtcNow - utcCreated > LifeSpan;
        }

        public bool GetAgentSession(string sessionClientId, out DomainAgentSessionHostBase sessionHost)
        {
            return hostList.TryGetValue(sessionClientId, out sessionHost);
        }

        private DomainAgentSessionClient ConnectionFactory_OnConnectionRequest(ServerAgentDefinition agent)
        {
            var host = OnCreateHost(agent);
            hostList[host.SessionClientId] = host;

            return host.SessionClient;
        }

        private void PackageClient_OnPushProjectPackage(string filename, RemoteTaskCompletionSource<object> taskHandle)
        {
            Task.Run(async () => {
                var metadata = await ProjectPackageTools.GetMetadataAsync(filename);
                if (metadata == null) throw new ApplicationException($"Invalid Project Package '{filename}'! No metadata found.");

                await projectPackages.Add(filename);
                PushedProjectPackageList.Add(new PackageReference(metadata.Id, metadata.Version));
                return (object)null;
            }).ContinueWith(taskHandle.FromTask);
        }

        private void PackageClient_OnPushApplicationPackage(string filename, RemoteTaskCompletionSource<object> taskHandle)
        {
            Task.Run(async () => {
                await applicationPackages.Add(filename);
                return (object)null;
            }).ContinueWith(taskHandle.FromTask);
        }

        private void PackageClient_OnPullProjectPackage(string id, string version, RemoteTaskCompletionSource<string> taskHandle)
        {
            Task.Run(async () => {
                if (!projectPackages.TryGet(id, version, out var packageFilename))
                    throw new ApplicationException($"Project Package '{id}.{version}' not found!");

                return await Task.FromResult(packageFilename);
            }).ContinueWith(taskHandle.FromTask);
        }

        private void PackageClient_OnPullApplicationPackage(string id, string version, RemoteTaskCompletionSource<string> taskHandle)
        {
            Task.Run(async () => {
                if (!applicationPackages.TryGet(id, version, out var packageFilename))
                    throw new ApplicationException($"Application Package '{id}.{version}' not found!");

                return await Task.FromResult(packageFilename);
            }).ContinueWith(taskHandle.FromTask);
        }
    }
}
