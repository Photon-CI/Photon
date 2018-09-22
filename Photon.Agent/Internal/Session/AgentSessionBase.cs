using log4net;
using Photon.Agent.Internal.Applications;
using Photon.Agent.Internal.Workers;
using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Projects;
using Photon.Framework.Server;
using Photon.Framework.Tools;
using Photon.Framework.Variables;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Session
{
    internal abstract class AgentSessionBase : IAgentSession
    {
        public event EventHandler ReleaseEvent;

        protected readonly CancellationTokenSource TokenSource;
        protected readonly ConcurrentDictionary<string, Task> taskList;
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
        protected ApplicationHost Applications {get;}
        public MessageTransceiver Transceiver {get;}
        public SessionOutput Output {get;}
        public VariableSetCollection ServerVariables {get; set;}
        public VariableSetCollection AgentVariables {get; set;}
        public bool IsReleased {get; set;}

        protected ILog Log => _log.Value;

        protected Worker WorkerHandle {get;}


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
            taskList = new ConcurrentDictionary<string, Task>(StringComparer.Ordinal);

            _log = new Lazy<ILog>(() => LogManager.GetLogger(GetType()));
            Output = new SessionOutput(transceiver, ServerSessionId, SessionClientId);
            WorkDirectory = Path.Combine(Configuration.WorkDirectory, SessionId);
            ContentDirectory = Path.Combine(WorkDirectory, "content");
            BinDirectory = Path.Combine(WorkDirectory, "bin");

            Applications = new ApplicationHost();

            WorkerHandle = new Worker {
                Filename = Path.Combine(Configuration.AssemblyPath, "Worker", "Photon.Worker.exe"),
                ShowConsole = true,
            };
        }

        public virtual void Dispose()
        {
            if (!IsReleased)
                Log.Error("Session was disposed without being released!");
            
            TokenSource?.Dispose();
            Applications.Dispose();
        }

        //public void Cancel()
        //{
        //    TokenSource?.Cancel();
        //}

        //public virtual void OnSessionBegin() {}

        //public virtual void OnSessionEnd() {}

        public virtual async Task InitializeAsync()
        {
            AgentVariables = await PhotonAgent.Instance.Variables.GetCollection();

            Directory.CreateDirectory(WorkDirectory);
            Directory.CreateDirectory(ContentDirectory);
            Directory.CreateDirectory(BinDirectory);
        }

        //public abstract Task RunTaskAsync(string taskName, string taskSessionId);

        public abstract Task CompleteAsync();

        public async Task ReleaseAsync()
        {
            if (IsReleased) return;

            IsReleased = true;
            TimeReleased = DateTime.UtcNow;
            Output?.Close();
            OnReleased();

            using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(8))) {
                await Transceiver.FlushAsync(tokenSource.Token);
                Transceiver.Stop();
            }

            try {
                WorkerHandle.Disconnect();
            }
            catch (Exception error) {
                Log.Error($"An error occurred while unloading session [{SessionId}]!", error);
            }

            try {
                await CleanupWorkDir();
            }
            catch (Exception error) {
                Log.Error($"Failed to clean the work directory! {error.UnfoldMessages()}");
            }

            try {
                var maxAppCount = PhotonAgent.Instance.AgentConfiguration.Value.Applications.MaxCount;
                PhotonAgent.Instance.ApplicationMgr.ApplyRetentionPolicy(maxAppCount);
                PhotonAgent.Instance.ApplicationMgr.Save();
            }
            catch (Exception error) {
                Log.Error("Failed to apply application revision policy!", error);
            }
        }

        public async Task AbortAsync()
        {
            TokenSource.Cancel();

            var timeout = TimeSpan.FromSeconds(30);
            var tasks = taskList.Values.ToArray();

            using (var timeoutTokenSource = new TimeoutTokenSource(timeout)) {
                await Task.Run(async () => await Task.WhenAll(tasks), timeoutTokenSource.Token);
            }

            await ReleaseAsync();
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

        protected void StartWorker()
        {
            // TODO
            WorkerHandle.Username = null;
            WorkerHandle.Password = null;
            WorkerHandle.LoadUserProfile = false;

            WorkerHandle.Start();
        }

        private async Task CleanupWorkDir()
        {
            const int retryDelay = 3000;
            const int retryCount = 3;

            var attempt = 1;
            var successful = false;
            Exception lastError = null;
            while (!successful && attempt < retryCount) {
                try {
                    PathEx.DestoryDirectory(WorkDirectory);
                    successful = true;
                }
                catch (Exception error) {
                    lastError = error;
                }

                if (!successful) {
                    await Task.Delay(retryDelay);
                    attempt++;
                }
            }

            if (!successful) {
                throw new ApplicationException($"Failed after {retryCount} attempts!", lastError);
            }
        }
    }
}
