using log4net;
using Photon.Library;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class ServerBuildSession : IServerSession
    {
        private static ILog Log = LogManager.GetLogger(typeof(ServerBuildSession));

        private readonly ServerDomain domain;
        private readonly DateTime utcCreated;
        private bool isReleased;

        public string Id {get;}
        public ScriptBuildDefinition BuildDefinition {get;}
        public TimeSpan MaxLifespan {get; set;}
        public Exception Exception {get; set;}

        public string WorkDirectory {get; private set;}


        public ServerBuildSession(ProjectDefinition project, ProjectJobDefinition job)
        {
            Id = Guid.NewGuid().ToString("N");
            utcCreated = DateTime.UtcNow;
            MaxLifespan = TimeSpan.FromMinutes(60);
            domain = new ServerDomain();

            BuildDefinition = new ScriptBuildDefinition {
                SessionId = Id,
                Project = {
                    Name = project.Name,
                    Source = project.Source,
                    Job = job,
                }
            };
        }

        public void Dispose()
        {
            Release();
        }

        public void Run()
        {
            WorkDirectory = Path.Combine(Configuration.WorkDirectory, Id);

            PrepareWorkDirectory();

            var preBuildCommand = BuildDefinition.Project.Job.PreBuild;
            if (!string.IsNullOrWhiteSpace(preBuildCommand))
                RunCommandLine(preBuildCommand);

            var assemblyFilename = Path.Combine(WorkDirectory, BuildDefinition.Project.Job.Assembly);
            if (!File.Exists(assemblyFilename))
                throw new FileNotFoundException($"The assembly file '{assemblyFilename}' could not be found!");

            domain.Initialize(assemblyFilename);
            domain.RunScript(BuildDefinition.Project.Job.Script);

            var postBuildCommand = BuildDefinition.Project.Job.PostBuild;
            if (!string.IsNullOrWhiteSpace(postBuildCommand))
                RunCommandLine(postBuildCommand);
        }

        public void Release()
        {
            domain?.Dispose();
            DestoryDirectory(WorkDirectory);
            isReleased = true;
        }

        public bool IsExpired()
        {
            if (isReleased) return true;

            var elapsed = DateTime.UtcNow - utcCreated;
            return elapsed > MaxLifespan;
        }

        private void PrepareWorkDirectory()
        {
            Directory.CreateDirectory(WorkDirectory);

            var sourceType = BuildDefinition.Project.Source.Type;

            if (string.Equals(sourceType, "fs")) {
                CopyDirectory(BuildDefinition.Project.Source.Source, WorkDirectory);
                return;
            }

            if (string.Equals(sourceType, "git")) {
                // TODO: Load Repository
                throw new NotImplementedException();
            }

            throw new ApplicationException($"Unknown source type '{sourceType}'!");
        }

        private void RunCommandLine(string command)
        {
            var result = ProcessRunner.Run(WorkDirectory, command);

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
            foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)) {
                try {
                    File.Delete(file);
                }
                catch (Exception error) {
                    Log.Warn($"Failed to delete file '{file}'! {error.Message}");
                }
            }

            try {
                Directory.Delete(WorkDirectory, true);
            }
            catch (Exception error) {
                Log.Warn($"Failed to remove directory '{WorkDirectory}'! {error.Message}");
            }
        }
    }
}
