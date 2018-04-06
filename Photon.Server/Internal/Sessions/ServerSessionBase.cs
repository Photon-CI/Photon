using log4net;
using Photon.Framework;
using Photon.Framework.Scripts;
using Photon.Library;
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
        protected ServerDomain Domain {get; private set;}
        public bool Complete {get; set;}
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
            Domain = new ServerDomain();
            Output = new ScriptOutput();

            _log = new Lazy<ILog>(() => LogManager.GetLogger(GetType()));
            WorkDirectory = Path.Combine(Configuration.WorkDirectory, SessionId);
        }

        public virtual void Dispose()
        {
            if (!isReleased)
                ReleaseAsync().GetAwaiter().GetResult();

            Domain?.Dispose();
            Domain = null;
        }

        public abstract Task RunAsync();

        public async Task ReleaseAsync()
        {
            if (isReleased) return;
            isReleased = true;

            // TODO: Hack! A ThreadAbortException
            //  will be called if we immediately
            //  close the AppDomain.
            await Task.Delay(200);

            Complete = true;
            utcReleased = DateTime.UtcNow;
            Domain?.Unload();

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
            Directory.CreateDirectory(WorkDirectory);
        }

        protected void RunCommandLine(string command)
        {
            var result = ProcessRunner.Run(WorkDirectory, command, Output);

            if (result.ExitCode != 0)
                throw new ApplicationException("Process terminated with a non-zero exit code!");
        }
    }
}
