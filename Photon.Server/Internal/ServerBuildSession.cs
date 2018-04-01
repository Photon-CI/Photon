using Photon.Framework.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class ServerBuildSession : ServerSessionBase
    {
        public ServerBuildSession(ProjectDefinition project, ProjectJobDefinition job) : base(project, job) {}

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

        public override async Task RunAsync()
        {
            var errorList = new Lazy<List<Exception>>();
            var abort = false;

            var preBuildCommand = Context.Job.PreBuild;
            if (!string.IsNullOrWhiteSpace(preBuildCommand)) {
                //Log.Debug("Running script Pre-Build command...");

                try {
                    RunCommandLine(preBuildCommand);
                }
                catch (Exception error) {
                    errorList.Value.Add(new ApplicationException($"Script Pre-Build command failed! [{Id}]", error));
                    //Log.Error($"Script Pre-Build command failed! [{Id}]", error);
                    Context.Output.AppendLine($"An error occurred while executing the script Pre-Build command! {error.Message} [{Id}]");
                    abort = true;
                }
            }

            var assemblyFilename = Path.Combine(Context.WorkDirectory, Context.Job.Assembly);

            if (!File.Exists(assemblyFilename)) {
                errorList.Value.Add(new ApplicationException($"The assembly file '{assemblyFilename}' could not be found!"));
                Context.Output.AppendLine($"The assembly file '{assemblyFilename}' could not be found!");
                //throw new FileNotFoundException($"The assembly file '{assemblyFilename}' could not be found!");
                abort = true;
            }

            if (!abort) {
                try {
                    Domain.Initialize(assemblyFilename);
                }
                catch (Exception error) {
                    errorList.Value.Add(new ApplicationException($"Script initialization failed! [{Id}]", error));
                    //Log.Error($"Script initialization failed! [{Id}]", error);
                    Context.Output.AppendLine($"An error occurred while initializing the script! {error.Message} [{Id}]");
                    abort = true;
                }
            }

            if (!abort) {
                try {
                    var result = await Domain.RunScript(Context);
                    if (!result.Successful) throw new ApplicationException(result.Message);
                }
                catch (Exception error) {
                    errorList.Value.Add(new ApplicationException($"Script execution failed! [{Id}]", error));
                    //Log.Error($"Script execution failed! [{Id}]", error);
                    Context.Output.AppendLine($"An error occurred while executing the script! {error.Message} [{Id}]");
                }
            }

            var postBuildCommand = Context.Job.PostBuild;
            if (!string.IsNullOrWhiteSpace(postBuildCommand)) {
                try {
                    RunCommandLine(postBuildCommand);
                }
                catch (Exception error) {
                    errorList.Value.Add(new ApplicationException($"Script Post-Build command failed! [{Id}]", error));
                    //Log.Error($"Script Post-Build command failed! [{Id}]", error);
                    Context.Output.AppendLine($"An error occurred while executing the script Post-Build command! {error.Message} [{Id}]");
                }
            }

            if (errorList.IsValueCreated)
                throw new AggregateException(errorList.Value);
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
