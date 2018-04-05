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
        private bool isReleased;

        public string SessionId {get; set;}
        public string WorkPath {get; set;}
        public string AssemblyFile {get; set;}
        protected AgentSessionDomain Domain {get; set;}
        public TimeSpan MaxLifespan {get; set;}
        public Exception Exception {get; set;}
        protected ILog Log => _log.Value;


        public AgentSessionBase()
        {
            SessionId = Guid.NewGuid().ToString("N");
            utcCreated = DateTime.UtcNow;
            MaxLifespan = TimeSpan.FromMinutes(60);

            _log = new Lazy<ILog>(() => LogManager.GetLogger(GetType()));
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
            Domain?.Dispose();

            if (!isReleased) {
                var workDirectory = WorkPath;
                try {
                    await Task.Run(() => FileUtils.DestoryDirectory(workDirectory));
                }
                catch (AggregateException errors) {
                    errors.Flatten().Handle(e => {
                        if (e is IOException ioError) {
                            Log.Warn(errors.Message);
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
            if (isReleased) return true;

            var elapsed = DateTime.UtcNow - utcCreated;
            return elapsed > MaxLifespan;
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
