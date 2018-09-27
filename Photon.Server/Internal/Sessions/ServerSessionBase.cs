using log4net;
using Photon.Framework.Projects;
using Photon.Framework.Server;
using Photon.Framework.Tasks;
using Photon.Framework.Tools;
using Photon.Framework.Variables;
using Photon.Server.Internal.AgentConnections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Photon.Library.Packages;

namespace Photon.Server.Internal.Sessions
{
    internal abstract class ServerSessionBase : IServerSession
    {
        //private readonly ConcurrentDictionary<string, DomainAgentSessionHostBase> hostList;

        public event EventHandler ReleaseEvent;

        private readonly Lazy<ILog> _log;
        public ServerContext Context {get;}
        public DateTime TimeCreated {get;}
        public DateTime? TimeReleased {get; private set;}
        public bool IsUserAborted {get; private set;}

        public string SessionId {get;}
        public Project Project {get; set;}
        public string WorkDirectory {get;}
        public string BinDirectory {get;}
        public string ContentDirectory {get;}
        //protected ServerDomain Domain {get; set;}
        public bool IsComplete {get; set;}
        public TimeSpan CacheSpan {get; set;}
        public TimeSpan LifeSpan {get; set;}
        public Exception Exception {get; set;}
        public ScriptOutput Output {get;}
        protected AgentConnectionManager ConnectionFactory {get;}
        //public PackageHost Packages {get;}
        public TaskResult Result {get; private set;}
        public VariableSetCollection ServerVariables {get; private set;}
        public List<PackageReference> PushedProjectPackages {get; set;}
        public List<PackageReference> PushedApplicationPackages {get; set;}
        public bool IsReleased {get; private set;}

        protected ILog Log => _log.Value;

        protected readonly CancellationTokenSource TokenSource;


        protected ServerSessionBase(ServerContext context)
        {
            this.Context = context;

            SessionId = Guid.NewGuid().ToString("N");
            TimeCreated = DateTime.UtcNow;
            CacheSpan = TimeSpan.FromHours(8);
            LifeSpan = TimeSpan.FromHours(12);
            Output = new ScriptOutput();

            _log = new Lazy<ILog>(() => LogManager.GetLogger(GetType()));
            //hostList = new ConcurrentDictionary<string, DomainAgentSessionHostBase>(StringComparer.Ordinal);
            WorkDirectory = Path.Combine(Configuration.WorkDirectory, SessionId);
            BinDirectory = Path.Combine(WorkDirectory, "bin");
            ContentDirectory = Path.Combine(WorkDirectory, "content");

            ConnectionFactory = new AgentConnectionManager();
            //ConnectionFactory.OnConnectionRequest += ConnectionFactory_OnConnectionRequest;

            //Packages = new PackageHost();

            PushedProjectPackages = new List<PackageReference>();
            PushedApplicationPackages = new List<PackageReference>();

            TokenSource = new CancellationTokenSource();
        }

        public virtual async Task InitializeAsync()
        {
            //Packages.ProjectId = Project?.Id;

            ServerVariables = await PhotonServer.Instance.Variables.GetCollection();

            Directory.CreateDirectory(WorkDirectory);
            Directory.CreateDirectory(BinDirectory);
            Directory.CreateDirectory(ContentDirectory);
        }

        public virtual void Dispose()
        {
            if (!IsReleased) {
                try {
                    Release();
                }
                catch {}
            }

            TokenSource?.Dispose();
            ConnectionFactory?.Dispose();
            //Packages?.Dispose();
            //Domain?.Dispose();
            //Domain = null;
        }

        public abstract Task RunAsync();

        public void Release()
        {
            if (IsReleased) return;
            IsReleased = true;

            TimeReleased = DateTime.UtcNow;
            OnReleased();

            //foreach (var host in hostList.Values) {
            //    try {
            //        host.Stop();
            //    }
            //    catch (Exception error) {
            //        Log.Error("Failed to stop host!", error);
            //    }

            //    try {
            //        host.Dispose();
            //    }
            //    catch (Exception error) {
            //        Log.Error("Failed to dispose host!", error);
            //    }
            //}
            //hostList.Clear();

            //if (Domain != null) {
            //    try {
            //        await Domain.Unload(true);
            //    }
            //    catch (Exception error) {
            //        Log.Error($"An error occurred while unloading the session domain [{SessionId}]!", error);
            //    }

            //    try {
            //        Domain.Dispose();
            //    }
            //    catch (Exception error) {
            //        Log.Error($"An error occurred while disposing the session domain [{SessionId}]!", error);
            //    }

            //    Domain = null;
            //}

            try {
                PathEx.DestoryDirectory(WorkDirectory);
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

            //foreach (var host in hostList.Values) {
            //    try {
            //        host.Abort();
            //    }
            //    catch {}
            //}

            Complete(TaskResult.Cancel());
            Release();
        }

        //public async Task AbortAsync()
        //{
        //    IsUserAborted = true;
        //    TokenSource.Cancel();

        //    //foreach (var host in hostList.Values) {
        //    //    try {
        //    //        host.Abort();
        //    //    }
        //    //    catch {}
        //    //}

        //    Complete(TaskResult.Cancel());
        //    Release();
        //}

        public virtual void Complete(TaskResult result)
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

        //public bool GetAgentSession(string sessionClientId, out DomainAgentSessionHostBase sessionHost)
        //{
        //    return hostList.TryGetValue(sessionClientId, out sessionHost);
        //}

        protected void OnReleased()
        {
            ReleaseEvent?.Invoke(this, EventArgs.Empty);
        }

        //public DomainAgentSessionClient GetClient(ServerAgent agent)
        //{
        //    var host = OnCreateHost(agent);
        //    hostList[host.SessionClientId] = host;

        //    return host.SessionClient;
        //}
    }
}
