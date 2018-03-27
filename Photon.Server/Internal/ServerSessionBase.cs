using log4net;
using Photon.Framework.Projects;
using Photon.Framework.Scripts;
using Photon.Library;
using System;
using System.IO;

namespace Photon.Server.Internal
{
    internal abstract class ServerSessionBase : IServerSession
    {
        private readonly Lazy<ILog> _log;
        private readonly DateTime utcCreated;
        private bool isReleased;

        public string Id {get;}
        protected ServerDomain Domain {get;}
        public ScriptContext Context {get;}
        public TimeSpan MaxLifespan {get; set;}
        public Exception Exception {get; set;}
        protected ILog Log => _log.Value;


        public ServerSessionBase(ProjectDefinition project, ProjectJobDefinition job)
        {
            Id = Guid.NewGuid().ToString("N");
            utcCreated = DateTime.UtcNow;
            MaxLifespan = TimeSpan.FromMinutes(60);
            Domain = new ServerDomain();

            Context = new ScriptContext(Id, PhotonServer.Instance, project, job);
            _log = new Lazy<ILog>(() => LogManager.GetLogger(GetType()));
        }

        public void Dispose()
        {
            Release();
        }

        public abstract void Run();

        public void Release()
        {
            Domain?.Dispose();
            //domain = null;

            if (!isReleased) {
                DestoryDirectory(Context.WorkDirectory);
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
            Directory.CreateDirectory(Context.WorkDirectory);
        }

        protected void RunCommandLine(string command)
        {
            var result = ProcessRunner.Run(Context.WorkDirectory, command);

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

        private void DestoryDirectory(string path)
        {
            try {
                DestoryDirectoryContents(path);
            }
            catch {}

            try {
                Directory.Delete(Context.WorkDirectory, true);
            }
            catch (Exception error) {
                Log.Warn($"Failed to remove directory '{Context.WorkDirectory}'! {error.Message}");
            }
        }

        private void DestoryDirectoryContents(string path)
        {
            foreach (var subdir in Directory.GetDirectories(path)) {
                DestoryDirectoryContents(subdir);

                try {
                    Directory.Delete(subdir);
                }
                catch {}
            }

            foreach (var file in Directory.GetFiles(path)) {
                try {
                    var attr = File.GetAttributes(file);
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        File.SetAttributes(file, attr ^ FileAttributes.ReadOnly);

                    File.Delete(file);
                }
                catch (Exception error) {
                    Log.Warn($"Failed to delete file '{file}'! {error.Message}");
                }
            }
        }
    }
}
