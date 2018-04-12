using log4net;
using Photon.Framework;
using Photon.Framework.Server;
using Photon.Framework.Sessions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal abstract class ServerSessionBase : IServerSession
    {
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
        protected ILog Log => _log.Value;


        protected ServerSessionBase()
        {
            SessionId = Guid.NewGuid().ToString("N");
            utcCreated = DateTime.UtcNow;
            CacheSpan = TimeSpan.FromHours(1);
            LifeSpan = TimeSpan.FromHours(8);
            Output = new ScriptOutput();

            _log = new Lazy<ILog>(() => LogManager.GetLogger(GetType()));
            WorkDirectory = Path.Combine(Configuration.WorkDirectory, SessionId);
            BinDirectory = Path.Combine(WorkDirectory, "bin");
            ContentDirectory = Path.Combine(WorkDirectory, "content");
        }

        public virtual void Dispose()
        {
            if (!isReleased)
                ReleaseAsync().GetAwaiter().GetResult();

            Domain?.Dispose();
            Domain = null;
        }

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

            if (Domain != null)
                await Domain.Unload(true);

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

        public void Complete()
        {
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
    }
}
