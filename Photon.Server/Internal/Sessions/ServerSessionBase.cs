using log4net;
using Photon.Framework.Domain;
using Photon.Framework.Server;
using Photon.Framework.Tasks;
using Photon.Framework.Tools;
using Photon.Framework.Variables;
using Photon.Server.Internal.Packages;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal abstract class ServerSessionBase : IServerSession
    {
        private readonly ConcurrentDictionary<string, DomainAgentSessionHostBase> hostList;

        public event EventHandler PreBuildEvent;
        public event EventHandler PostBuildEvent;
        public event EventHandler ReleaseEvent;

        private readonly Lazy<ILog> _log;
        public DateTime TimeCreated {get;}
        public DateTime? TimeReleased {get; private set;}
        public bool IsUserAborted {get; private set;}

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
        public PackageHost Packages {get;}
        public TaskResult Result {get; private set;}
        public VariableSetCollection Variables {get; private set;}
        public bool IsReleased {get; private set;}

        protected ILog Log => _log.Value;

        protected readonly CancellationTokenSource TokenSource;


        protected ServerSessionBase()
        {
            SessionId = Guid.NewGuid().ToString("N");
            TimeCreated = DateTime.UtcNow;
            CacheSpan = TimeSpan.FromHours(8);
            LifeSpan = TimeSpan.FromHours(12);
            Output = new ScriptOutput();

            _log = new Lazy<ILog>(() => LogManager.GetLogger(GetType()));
            hostList = new ConcurrentDictionary<string, DomainAgentSessionHostBase>(StringComparer.Ordinal);
            WorkDirectory = Path.Combine(Configuration.WorkDirectory, SessionId);
            BinDirectory = Path.Combine(WorkDirectory, "bin");
            ContentDirectory = Path.Combine(WorkDirectory, "content");

            ConnectionFactory = new DomainConnectionFactory();
            ConnectionFactory.OnConnectionRequest += ConnectionFactory_OnConnectionRequest;

            Packages = new PackageHost();

            TokenSource = new CancellationTokenSource();
        }

        public virtual async Task InitializeAsync()
        {
            Variables = await PhotonServer.Instance.Variables.GetCollection();
        }

        public virtual void Dispose()
        {
            if (!IsReleased)
                ReleaseAsync().GetAwaiter().GetResult();

            TokenSource?.Dispose();
            ConnectionFactory?.Dispose();
            Packages?.Dispose();
            Domain?.Dispose();
            Domain = null;
        }

        protected abstract DomainAgentSessionHostBase OnCreateHost(ServerAgent agent);

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
            if (IsReleased) return;
            IsReleased = true;

            TimeReleased = DateTime.UtcNow;
            OnReleased();

            foreach (var host in hostList.Values) {
                try {
                    host.Stop();
                }
                catch (Exception error) {
                    Log.Error("Failed to stop host!", error);
                }

                try {
                    host.Dispose();
                }
                catch (Exception error) {
                    Log.Error("Failed to dispose host!", error);
                }
            }
            hostList.Clear();

            if (Domain != null) {
                try {
                    await Domain.Unload(true);
                }
                catch (Exception error) {
                    Log.Error($"An error occurred while unloading the session domain [{SessionId}]!", error);
                }

                try {
                    Domain.Dispose();
                }
                catch (Exception error) {
                    Log.Error($"An error occurred while disposing the session domain [{SessionId}]!", error);
                }

                Domain = null;
            }

            var workDirectory = WorkDirectory;
            try {
                await Task.Run(() => PathEx.DestoryDirectory(workDirectory));
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
            IsUserAborted = true;
            TokenSource.Cancel();

            foreach (var host in hostList.Values)
                host.Abort();

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
            if (TimeReleased.HasValue) {
                if (DateTime.UtcNow - TimeReleased > CacheSpan)
                    return true;
            }

            return DateTime.UtcNow - TimeCreated > LifeSpan;
        }

        public bool GetAgentSession(string sessionClientId, out DomainAgentSessionHostBase sessionHost)
        {
            return hostList.TryGetValue(sessionClientId, out sessionHost);
        }

        public void OnPreBuildEvent()
        {
            PreBuildEvent?.Invoke(this, EventArgs.Empty);
        }

        public void OnPostBuildEvent()
        {
            PostBuildEvent?.Invoke(this, EventArgs.Empty);
        }

        protected void OnReleased()
        {
            ReleaseEvent?.Invoke(this, EventArgs.Empty);
        }

        private DomainAgentSessionClient ConnectionFactory_OnConnectionRequest(ServerAgent agent)
        {
            var host = OnCreateHost(agent);
            hostList[host.SessionClientId] = host;

            return host.SessionClient;
        }
    }
}
