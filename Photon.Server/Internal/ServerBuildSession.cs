using Photon.Framework.Projects;
using System;
using System.IO;

namespace Photon.Server.Internal
{
    internal class ServerBuildSession : ServerSessionBase
    {
        public ServerBuildSession(ProjectDefinition project, ProjectJobDefinition job) : base(project, job)
        {
            //...
        }

        public override void PrepareWorkDirectory()
        {
            base.PrepareWorkDirectory();

            var sourceType = Context.Project.Source.Type;

            if (string.Equals(sourceType, "fs")) {
                CopyDirectory(Context.Project.Source.Source, Context.WorkDirectory);
                return;
            }

            if (string.Equals(sourceType, "git")) {
                // TODO: Load Repository
                throw new NotImplementedException();
            }

            throw new ApplicationException($"Unknown source type '{sourceType}'!");
        }

        public override void Run()
        {
            var preBuildCommand = Context.Job.PreBuild;
            if (!string.IsNullOrWhiteSpace(preBuildCommand))
                RunCommandLine(preBuildCommand);

            var assemblyFilename = Path.Combine(Context.WorkDirectory, Context.Job.Assembly);
            if (!File.Exists(assemblyFilename))
                throw new FileNotFoundException($"The assembly file '{assemblyFilename}' could not be found!");

            Domain.Initialize(assemblyFilename);

            var allScripts = Domain.GetScripts();

            Domain.RunScript(Context);

            var postBuildCommand = Context.Job.PostBuild;
            if (!string.IsNullOrWhiteSpace(postBuildCommand))
                RunCommandLine(postBuildCommand);
        }

        private void CopyDirectory(string sourcePath, string destPath)
        {
            foreach (var path in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) {
                var newPath = path.Replace(sourcePath, destPath);
                Directory.CreateDirectory(newPath);
            }

            foreach (var file in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)) {
                var newPath = file.Replace(sourcePath, destPath);

                try {
                    File.Copy(file, newPath, true);
                }
                catch (Exception error) {
                    Log.Warn($"Failed to copy file '{file}'! {error.Message}");
                }
            }
        }
    }
}
