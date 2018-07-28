using Photon.Agent.Internal.Git;
using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Agent;
using Photon.Framework.Domain;
using Photon.Framework.Projects;
using Photon.Library.GitHub;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Photon.Framework.Tools;

namespace Photon.Agent.Internal.Session
{
    internal class AgentBuildSession : AgentSessionBase
    {
        public string PreBuild {get; set;}
        public string GitRefspec {get; set;}
        public string TaskName {get; set;}
        public uint BuildNumber {get; set;}
        public GithubCommit Commit {get; set;}


        public AgentBuildSession(MessageTransceiver transceiver, string serverSessionId, string sessionClientId)
            : base(transceiver, serverSessionId, sessionClientId) {}

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            LoadProjectSource();

            LoadProjectAssembly();
        }

        public override async Task RunTaskAsync(string taskName, string taskSessionId)
        {
            using (var contextOutput = new DomainOutput()) {
                contextOutput.OnWrite += (text, color) => Output.Write(text, color);
                contextOutput.OnWriteLine += (text, color) => Output.WriteLine(text, color);
                contextOutput.OnWriteRaw += (text) => Output.WriteRaw(text);

                var context = new AgentBuildContext {
                    Project = Project,
                    Agent = Agent,
                    AssemblyFilename = AssemblyFilename,
                    GitRefspec = GitRefspec,
                    TaskName = taskName,
                    WorkDirectory = WorkDirectory,
                    ContentDirectory = ContentDirectory,
                    BinDirectory = BinDirectory,
                    BuildNumber = BuildNumber,
                    Output = contextOutput,
                    Packages = PackageClient,
                    ServerVariables = ServerVariables,
                    AgentVariables = AgentVariables,
                };

                var githubSource = Project?.Source as ProjectGithubSource;
                var notifyGithub = githubSource != null && githubSource.NotifyOrigin == NotifyOrigin.Agent && Commit != null;
                CommitStatusUpdater su = null;
                CommitStatus status;

                if (notifyGithub) {
                    su = new CommitStatusUpdater {
                        Username = githubSource.Username,
                        Password = githubSource.Password,
                        StatusUrl = Commit.StatusesUrl,
                        Sha = Commit.Sha,
                    };

                    status = new CommitStatus {
                        State = CommitStates.Pending,
                        Context = "Photon",
                        Description = "Build in progress..."
                    };

                    await su.Post(status);
                }

                status = new CommitStatus {
                    Context = "Photon",
                };

                try {
                    await Domain.RunBuildTask(context, TokenSource.Token);

                    if (notifyGithub) {
                        status.State = CommitStates.Success;
                        status.Description = "Build Successful.";
                        await su.Post(status);
                    }
                }
                catch (Exception error) {
                    Exception = error;

                    if (notifyGithub) {
                        status.State = CommitStates.Failure;
                        status.Description = "Build Failed!";
                        await su.Post(status);
                    }

                    throw;
                }
            }
        }

        private void LoadProjectSource()
        {
            if (Project.Source is ProjectFileSystemSource fsSource) {
                Output.WriteLine($"Copying File-System directory '{fsSource.Path}' to work content directory.", ConsoleColor.DarkCyan);
                CopyDirectory(fsSource.Path, ContentDirectory);
                Output.WriteLine("Copy completed successfully.", ConsoleColor.DarkGreen);
                return;
            }

            if (Project.Source is ProjectGithubSource githubSource) {
                Output.WriteLine($"Cloning Git Repository '{githubSource.CloneUrl}' to work content directory.", ConsoleColor.DarkCyan);

                RepositoryHandle handle = null;
                try {
                    handle = GetRepositoryHandle(githubSource.CloneUrl, TimeSpan.FromMinutes(1), TokenSource.Token)
                        .GetAwaiter().GetResult();

                    handle.Username = githubSource.Username;
                    handle.Password = githubSource.Password;
                    handle.UseCommandLine = githubSource.UseCommandLine;
                    handle.CommandLineExe = githubSource.CommandLineExe;
                    handle.Output = Output;

                    handle.Checkout(GitRefspec);

                    Output.WriteLine("Copying repository to work content directory.", ConsoleColor.DarkCyan);
                    CopyDirectory(handle.Source.RepositoryPath, ContentDirectory);
                    Output.WriteLine("Copy completed successfully.", ConsoleColor.DarkGreen);
                }
                finally {
                    handle?.Dispose();
                }
                return;
            }

            throw new ApplicationException($"Unknown source type '{Project.Source?.GetType().Name}'!");
        }

        private async Task<RepositoryHandle> GetRepositoryHandle(string url, TimeSpan timeout, CancellationToken token = default(CancellationToken))
        {
            var repositorySource = PhotonAgent.Instance.RepositorySources.GetOrCreate(url);

            using (var timeoutTokenSource = new CancellationTokenSource(timeout)) 
            using (var joinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, token)) {
                while (!joinedTokenSource.IsCancellationRequested) {
                    if (repositorySource.TryBegin(out var handle)) {
                        return handle;
                    }

                    await Task.Delay(200, joinedTokenSource.Token);
                }
            }

            throw new TimeoutException("A timeout occurred waiting for the repository.");
        }

        private void LoadProjectAssembly()
        {
            if (string.IsNullOrEmpty(AssemblyFilename)) {
                Output.WriteLine("No assembly filename defined!", ConsoleColor.DarkRed);
                throw new ApplicationException("Assembly filename is undefined!");
            }

            var errorList = new Lazy<List<Exception>>();
            var abort = false;

            if (!string.IsNullOrWhiteSpace(PreBuild)) {
                Output.WriteLine("Running Pre-Build Script...", ConsoleColor.DarkCyan);

                try {
                    RunCommandScript(PreBuild);
                }
                catch (Exception error) {
                    errorList.Value.Add(new ApplicationException($"Script Pre-Build failed! [{SessionId}]", error));
                    //Log.Error($"Script Pre-Build command failed! [{Id}]", error);
                    Output.WriteLine($"An error occurred while executing the Pre-Build script! {error.Message} [{SessionId}]", ConsoleColor.DarkYellow);
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
            new DirectoryCopy {
                SourceDirectory = sourcePath,
                DestinationDirectory = destPath,
                IgnoredDirectories = {
                    ".git",
                }
            }.Copy();
        }

        protected void RunCommandScript(string command)
        {
            var runInfo = ProcessRunInfo.FromCommand(command);
            runInfo.WorkingDirectory = ContentDirectory;
            var result = ProcessRunner.Run(runInfo, Output.Writer);

            if (result.ExitCode != 0)
                throw new ApplicationException("Process terminated with a non-zero exit code!");
        }
    }
}
