using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Agent;
using Photon.Framework.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Session
{
    internal class AgentBuildSession : AgentSessionBase
    {
        public string PreBuild {get; set;}
        public string GitRefspec {get; set;}
        public int BuildNumber {get; set;}


        public AgentBuildSession(MessageTransceiver transceiver, string serverSessionId) : base(transceiver, serverSessionId) {}

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            LoadProjectSource();

            LoadProjectAssembly();
        }

        public override async Task<TaskResult> RunTaskAsync(string taskName, string taskSessionId)
        {
            var context = new AgentBuildContext {
                Project = Project,
                AssemblyFilename = AssemblyFilename,
                GitRefspec = GitRefspec,
                TaskName = taskName,
                WorkDirectory = WorkDirectory,
                ContentDirectory = ContentDirectory,
                BinDirectory = BinDirectory,
                BuildNumber = BuildNumber,
                Output = Output.Writer,
                Packages = PackageClient,
            };

            return await Domain.RunBuildTask(context);
        }

        private void LoadProjectSource()
        {
            var sourceType = Project.SourceType;

            if (string.Equals(sourceType, "fs")) {
                Output.WriteLine($"Copying File-System directory '{Project.SourcePath}' to work content directory.", ConsoleColor.DarkCyan);
                CopyDirectory(Project.SourcePath, ContentDirectory);
                Output.WriteLine("Copy completed successfully.", ConsoleColor.DarkGreen);
                return;
            }

            if (string.Equals(sourceType, "git")) {
                Output.WriteLine("Cloning Git Repository '...' to work content directory.", ConsoleColor.DarkCyan);

                // TODO: Load Repository
                throw new NotImplementedException();
            }

            throw new ApplicationException($"Unknown source type '{sourceType}'!");
        }

        private void LoadProjectAssembly()
        {
            var errorList = new Lazy<List<Exception>>();
            var abort = false;

            var preBuildCommand = PreBuild;
            if (!string.IsNullOrWhiteSpace(preBuildCommand)) {
                Output.WriteLine("Running Pre-Build Command...", ConsoleColor.DarkCyan);

                try {
                    RunCommandLine(preBuildCommand);
                }
                catch (Exception error) {
                    errorList.Value.Add(new ApplicationException($"Script Pre-Build command failed! [{SessionId}]", error));
                    //Log.Error($"Script Pre-Build command failed! [{Id}]", error);
                    Output.WriteLine($"An error occurred while executing the script Pre-Build command! {error.Message} [{SessionId}]", ConsoleColor.DarkYellow);
                    abort = true;
                }
            }

            var assemblyFilename = Path.Combine(ContentDirectory, AssemblyFilename);

            if (!File.Exists(assemblyFilename)) {
                errorList.Value.Add(new ApplicationException($"The assembly file '{assemblyFilename}' could not be found!"));
                Output.WriteLine($"The assembly file '{assemblyFilename}' could not be found!", ConsoleColor.DarkYellow);
                abort = true;
            }

            // Shadow-Copy assembly folder
            string assemblyCopyFilename = null;
            try {
                var sourcePath = Path.GetDirectoryName(assemblyFilename);
                var assemblyName = Path.GetFileName(assemblyFilename);
                assemblyCopyFilename = Path.Combine(BinDirectory, assemblyName);
                CopyDirectory(sourcePath, BinDirectory);
            }
            catch (Exception error) {
                errorList.Value.Add(new ApplicationException($"Failed to shadow-copy assembly '{assemblyFilename}'!", error));
                Output.WriteLine($"Failed to shadow-copy assembly '{assemblyFilename}'!", ConsoleColor.DarkYellow);
                abort = true;
            }

            if (!abort) {
                try {
                    Domain = new AgentSessionDomain();
                    Domain.Initialize(assemblyCopyFilename);
                }
                catch (Exception error) {
                    errorList.Value.Add(new ApplicationException($"Failed to initialize Assembly! [{SessionId}]", error));
                    Output.WriteLine($"An error occurred while initializing the assembly! {error.Message} [{SessionId}]", ConsoleColor.DarkRed);
                    //abort = true;
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

        protected void RunCommandLine(string command)
        {
            var result = ProcessRunner.Run(ContentDirectory, command, Output.Writer);

            if (result.ExitCode != 0)
                throw new ApplicationException("Process terminated with a non-zero exit code!");
        }
    }
}
