using log4net;
using Photon.Framework;
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
        private bool isReleased;

        public string SessionId {get;}
        public string WorkPath {get;}
        protected ServerDomain Domain {get;}
        public TimeSpan MaxLifespan {get; set;}
        public Exception Exception {get; set;}
        protected ILog Log => _log.Value;


        public ServerSessionBase()
        {
            SessionId = Guid.NewGuid().ToString("N");
            utcCreated = DateTime.UtcNow;
            MaxLifespan = TimeSpan.FromMinutes(60);
            Domain = new ServerDomain();

            _log = new Lazy<ILog>(() => LogManager.GetLogger(GetType()));
        }

        public void Dispose()
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

        protected void RunCommandLine(string command)
        {
            var result = ProcessRunner.Run(WorkPath, command);

            if (result.ExitCode != 0)
                throw new ApplicationException("Process terminated with a non-zero exit code!");
        }

        private void CopyDirectory(string sourcePath, string destPath)
        {
            foreach (var path in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) {
                var newPath = path.Replace(sourcePath, destPath);
                Directory.CreateDirectory(newPath);
            }

            foreach (var path in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)) {
                var newPath = path.Replace(sourcePath, destPath);
                File.Copy(path, newPath, true);
            }
        }
    }
}
