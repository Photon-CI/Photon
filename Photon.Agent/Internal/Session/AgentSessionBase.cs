using log4net;
using Photon.Communication;
using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using Photon.Framework.Packages;
using Photon.Framework.Projects;
using Photon.Framework.Server;
using Photon.Framework.Variables;
using Photon.Library;
using Photon.Library.TcpMessages;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Session
{
    internal abstract class AgentSessionBase : IAgentSession
    {
        public event EventHandler ReleaseEvent;

        protected readonly CancellationTokenSource TokenSource;
        private readonly Lazy<ILog> _log;
        public DateTime TimeCreated {get;}
        public DateTime? TimeReleased {get; private set;}

        public string SessionId {get;}
        public string ServerSessionId {get;}
        public string SessionClientId {get;}
        public string WorkDirectory {get;}
        public string ContentDirectory {get;}
        public string BinDirectory {get;}
        public Project Project {get; set;}
        public ServerAgent Agent {get; set;}
        public string AssemblyFilename {get; set;}
        public TimeSpan CacheSpan {get; set;}
        public TimeSpan LifeSpan {get; set;}
        public Exception Exception {get; set;}
        protected AgentSessionDomain Domain {get; set;}
        protected DomainPackageClient PackageClient {get;}
        public MessageTransceiver Transceiver {get;}
        public SessionOutput Output {get;}
        public VariableSetCollection ServerVariables {get; set;}
        public VariableSetCollection AgentVariables {get; set;}
        public bool IsReleased {get; set;}

        protected ILog Log => _log.Value;


        protected AgentSessionBase(MessageTransceiver transceiver, string serverSessionId, string sessionClientId)
        {
            this.Transceiver = transceiver;
            this.ServerSessionId = serverSessionId;
            this.SessionClientId = sessionClientId;

            SessionId = Guid.NewGuid().ToString("N");
            TimeCreated = DateTime.UtcNow;
            CacheSpan = TimeSpan.FromHours(1);
            LifeSpan = TimeSpan.FromHours(8);
            TokenSource = new CancellationTokenSource();

            _log = new Lazy<ILog>(() => LogManager.GetLogger(GetType()));
            Output = new SessionOutput(transceiver, ServerSessionId, SessionClientId);
            WorkDirectory = Path.Combine(Configuration.WorkDirectory, SessionId);
            ContentDirectory = Path.Combine(WorkDirectory, "content");
            BinDirectory = Path.Combine(WorkDirectory, "bin");

            PackageClient = new DomainPackageClient();
            PackageClient.OnPushProjectPackage += PackageClient_OnPushProjectPackage;
            PackageClient.OnPushApplicationPackage += PackageClient_OnPushApplicationPackage;
            PackageClient.OnPullProjectPackage += PackageClient_OnPullProjectPackage;
            PackageClient.OnPullApplicationPackage += PackageClient_OnPullApplicationPackage;
        }

        public virtual void Dispose()
        {
            if (!IsReleased)
                Log.Error("Session was disposed without being released!");
            
            TokenSource?.Dispose();
            PackageClient?.Dispose();
            Domain?.Dispose();
        }

        public void Cancel()
        {
            TokenSource?.Cancel();
        }

        public virtual void OnSessionBegin() {}
        public virtual void OnSessionEnd() {}

        public virtual async Task InitializeAsync()
        {
            AgentVariables = await PhotonAgent.Instance.Variables.GetCollection();

            await Task.Run(() => {
                Directory.CreateDirectory(WorkDirectory);
                Directory.CreateDirectory(ContentDirectory);
                Directory.CreateDirectory(BinDirectory);
            });
        }

        public abstract Task RunTaskAsync(string taskName, string taskSessionId);

        public async Task ReleaseAsync()
        {
            if (IsReleased) return;

            IsReleased = true;
            TimeReleased = DateTime.UtcNow;
            OnReleased();

            using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(8))) {
                Transceiver.Stop(tokenSource.Token);
            }

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

            try {
                var _workDirectory = WorkDirectory;
                await Task.Run(() => FileUtils.DestoryDirectory(_workDirectory));
            }
            catch (AggregateException errors) {
                errors.Flatten().Handle(e => {
                    if (e is IOException ioError) {
                        Log.Warn(ioError.Message);
                        return true;
                    }

                    Log.Warn($"An error occurred while cleaning the work directory! {e.Message}");
                    return true;
                });
            }
            catch (Exception error) {
                Log.Warn($"An error occurred while cleaning the work directory! {error.Message}");
            }
        }

        public void Abort()
        {
            TokenSource.Cancel();

            // TODO: Wait?
        }

        public bool IsExpired()
        {
            if (TimeReleased.HasValue) {
                if (DateTime.UtcNow - TimeReleased > CacheSpan)
                    return true;
            }

            return DateTime.UtcNow - TimeCreated > LifeSpan;
        }

        protected void OnReleased()
        {
            ReleaseEvent?.Invoke(this, EventArgs.Empty);
        }

        private void PackageClient_OnPushProjectPackage(string filename, RemoteTaskCompletionSource taskHandle)
        {
            var packageRequest = new ProjectPackagePushRequest {
                ServerSessionId = ServerSessionId,
                Filename = filename,
            };

            Transceiver.Send(packageRequest)
                .GetResponseAsync()
                .ContinueWith(taskHandle.FromTask);
        }

        private void PackageClient_OnPushApplicationPackage(string filename, RemoteTaskCompletionSource taskHandle)
        {
            var packageRequest = new ApplicationPackagePushRequest {
                Filename = filename,
            };

            Transceiver.Send(packageRequest)
                .GetResponseAsync()
                .ContinueWith(taskHandle.FromTask);
        }

        private void PackageClient_OnPullProjectPackage(string id, string version, RemoteTaskCompletionSource<string> taskHandle)
        {
            var packageRequest = new ProjectPackagePullRequest {
                ProjectPackageId = id,
                ProjectPackageVersion = version,
            };

            Task.Run(async () => {
                var response = await Transceiver.Send(packageRequest)
                    .GetResponseAsync<ProjectPackagePullResponse>();

                return response.Filename;
            }).ContinueWith(taskHandle.FromTask);
        }

        private void PackageClient_OnPullApplicationPackage(string id, string version, RemoteTaskCompletionSource<string> taskHandle)
        {
            Task.Run(async () => {
                var packageRequest = new ApplicationPackagePullRequest {
                    PackageId = id,
                    PackageVersion = version,
                };

                var response = await Transceiver.Send(packageRequest)
                    .GetResponseAsync<ApplicationPackagePullResponse>();

                return response.Filename;
            }).ContinueWith(taskHandle.FromTask);
        }
    }
}
