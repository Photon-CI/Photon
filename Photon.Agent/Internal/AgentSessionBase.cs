using log4net;
using Photon.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Agent.Internal
{
    internal abstract class AgentSessionBase : IAgentSession
    {
        private readonly Lazy<ILog> _log;
        private readonly DateTime utcCreated;
        private DateTime? utcReleased;
        private bool isReleased;

        public string SessionId {get;}
        public string WorkPath {get;}
        public string AssemblyFile {get; set;}
        public TimeSpan CacheSpan {get; set;}
        public TimeSpan LifeSpan {get; set;}
        public Exception Exception {get; set;}
        protected AgentSessionDomain Domain {get; set;}
        protected ILog Log => _log.Value;


        protected AgentSessionBase()
        {
            SessionId = Guid.NewGuid().ToString("N");
            utcCreated = DateTime.UtcNow;
            CacheSpan = TimeSpan.FromHours(1);
            LifeSpan = TimeSpan.FromHours(8);

            _log = new Lazy<ILog>(() => LogManager.GetLogger(GetType()));
            WorkPath = Path.Combine(Configuration.WorkDirectory, SessionId);
        }

        public virtual void Dispose()
        {
            if (!isReleased)
                ReleaseAsync().GetAwaiter().GetResult();

            Domain?.Dispose();
        }

        public abstract Task RunAsync();

        public async Task ReleaseAsync()
        {
            utcReleased = DateTime.UtcNow;
            Domain?.Dispose();

            if (!isReleased) {
                var workDirectory = WorkPath;
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

        public virtual void PrepareWorkDirectory()
        {
            Directory.CreateDirectory(WorkPath);
        }

        private void DownloadPackage(string packageName, string version, string outputDirectory)
        {
            //...
            throw new NotImplementedException();
        }
    }
}
