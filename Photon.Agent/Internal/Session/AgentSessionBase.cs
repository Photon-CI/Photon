﻿using log4net;
using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Projects;
using Photon.Framework.Sessions;
using Photon.Framework.Tasks;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Session
{
    internal abstract class AgentSessionBase : IAgentSession
    {
        private readonly Lazy<ILog> _log;
        private readonly DateTime utcCreated;
        private DateTime? utcReleased;
        private bool isReleased;

        public Project Project {get; set;}
        public string AssemblyFilename {get; set;}
        public TimeSpan CacheSpan {get; set;}
        public TimeSpan LifeSpan {get; set;}
        public Exception Exception {get; set;}
        protected AgentSessionDomain Domain {get; set;}

        public string SessionId {get;}
        public string ServerSessionId {get;}
        public string WorkDirectory {get;}
        public string ContentDirectory {get;}
        public string BinDirectory {get;}
        public MessageTransceiver Transceiver {get;}
        public SessionOutput Output {get;}
        protected ILog Log => _log.Value;


        protected AgentSessionBase(MessageTransceiver transceiver, string serverSessionId)
        {
            this.Transceiver = transceiver;
            this.ServerSessionId = serverSessionId;

            SessionId = Guid.NewGuid().ToString("N");
            utcCreated = DateTime.UtcNow;
            CacheSpan = TimeSpan.FromHours(1);
            LifeSpan = TimeSpan.FromHours(8);

            _log = new Lazy<ILog>(() => LogManager.GetLogger(GetType()));
            Output = new SessionOutput(transceiver, serverSessionId);
            WorkDirectory = Path.Combine(Configuration.WorkDirectory, SessionId);
            ContentDirectory = Path.Combine(WorkDirectory, "content");
            BinDirectory = Path.Combine(WorkDirectory, "bin");
        }

        public virtual void Dispose()
        {
            if (!isReleased)
                ReleaseAsync().GetAwaiter().GetResult();

            Domain?.Dispose();
        }

        public virtual async Task InitializeAsync()
        {
            await Task.Run(() => {
                Directory.CreateDirectory(WorkDirectory);
                Directory.CreateDirectory(ContentDirectory);
                Directory.CreateDirectory(BinDirectory);
            });
        }

        public abstract Task<TaskResult> RunTaskAsync(string taskName, string taskSessionId);

        public async Task ReleaseAsync()
        {
            utcReleased = DateTime.UtcNow;
            Domain?.Unload(true);

            if (!isReleased) {
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

                isReleased = true;
            }
        }

        public bool IsExpired()
        {
            if (utcReleased.HasValue) {
                if (DateTime.UtcNow - utcReleased > CacheSpan)
                    return true;
            }

            return DateTime.UtcNow - utcCreated > LifeSpan;
        }

        //private void DownloadPackage(string packageName, string version, string outputDirectory)
        //{
        //    //...
        //    throw new NotImplementedException();
        //}
    }
}
