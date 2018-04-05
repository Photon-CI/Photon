using Photon.Server.Internal.Scripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerBuildSession : ServerSessionBase
    {
        public ServerBuildContext Context {get;}


        public ServerBuildSession(ServerBuildContext context)
        {
            this.Context = context;

            context.WorkDirectory = WorkDirectory;
            context.Output = Output;
        }

        public override void PrepareWorkDirectory()
        {
            base.PrepareWorkDirectory();

            var sourceType = Context.Project.SourceType;

            if (string.Equals(sourceType, "fs")) {
                Output.AppendLine($"Copying File-System directory '{Context.Project.SourcePath}' to work directory.");
                CopyDirectory(Context.Project.SourcePath, Context.WorkDirectory);
                Output.AppendLine("Copy completed successfully.");
                return;
            }

            if (string.Equals(sourceType, "git")) {
                Output.AppendLine("Cloning Git Repository '...' to work directory.");

                // TODO: Load Repository
                throw new NotImplementedException();
            }

            throw new ApplicationException($"Unknown source type '{sourceType}'!");
        }

        public override async Task RunAsync()
        {
            var errorList = new Lazy<List<Exception>>();
            var abort = false;

            var preBuildCommand = Context.Project.PreBuild;
            if (!string.IsNullOrWhiteSpace(preBuildCommand)) {
                //Log.Debug("Running script Pre-Build command...");

                try {
                    RunCommandLine(preBuildCommand);
                }
                catch (Exception error) {
                    errorList.Value.Add(new ApplicationException($"Script Pre-Build command failed! [{SessionId}]", error));
                    //Log.Error($"Script Pre-Build command failed! [{Id}]", error);
                    Context.Output.AppendLine($"An error occurred while executing the script Pre-Build command! {error.Message} [{SessionId}]");
                    abort = true;
                }
            }

            var assemblyFilename = Path.Combine(Context.WorkDirectory, Context.AssemblyFile);

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
                    errorList.Value.Add(new ApplicationException($"Script initialization failed! [{SessionId}]", error));
                    //Log.Error($"Script initialization failed! [{Id}]", error);
                    Context.Output.AppendLine($"An error occurred while initializing the script! {error.Message} [{SessionId}]");
                    abort = true;
                }
            }

            if (!abort) {
                try {
                    var result = await Domain.RunBuildScript(Context);
                    if (!result.Successful) throw new ApplicationException(result.Message);
                }
                catch (Exception error) {
                    errorList.Value.Add(new ApplicationException($"Script execution failed! [{SessionId}]", error));
                    //Log.Error($"Script execution failed! [{Id}]", error);
                    Context.Output.AppendLine($"An error occurred while executing the script! {error.Message} [{SessionId}]");
                }
            }

            var postBuildCommand = Context.Project.PostBuild;
            if (!string.IsNullOrWhiteSpace(postBuildCommand)) {
                try {
                    RunCommandLine(postBuildCommand);
                }
                catch (Exception error) {
                    errorList.Value.Add(new ApplicationException($"Script Post-Build command failed! [{SessionId}]", error));
                    //Log.Error($"Script Post-Build command failed! [{Id}]", error);
                    Context.Output.AppendLine($"An error occurred while executing the script Post-Build command! {error.Message} [{SessionId}]");
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
