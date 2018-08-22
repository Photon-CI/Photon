using Photon.Agent.Internal.Applications;
using Photon.Agent.Internal.Git;
using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Agent;
using Photon.Framework.Applications;
using Photon.Framework.Domain;
using Photon.Framework.Projects;
using Photon.Framework.Tools.Content;
using Photon.Library.GitHub;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Session
{
    internal class AgentBuildSession : AgentSessionBase
    {
        public string PreBuild {get; set;}
        public string GitRefspec {get; set;}
        public string TaskName {get; set;}
        public uint BuildNumber {get; set;}
        public GithubCommit SourceCommit {get; set;}
        public string CommitHash {get; private set;}
        public string CommitAuthor {get; private set;}
        public string CommitMessage {get; private set;}


        public AgentBuildSession(MessageTransceiver transceiver, string serverSessionId, string sessionClientId)
            : base(transceiver, serverSessionId, sessionClientId) {}

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await LoadProjectSource();

            LoadProjectAssembly();
        }

        public override async Task RunTaskAsync(string taskName, string taskSessionId)
        {
            using (var contextOutput = new DomainOutput()) {
                contextOutput.OnWrite += (text, color) => Output.Write(text, color);
                contextOutput.OnWriteLine += (text, color) => Output.WriteLine(text, color);
                contextOutput.OnWriteRaw += (text) => Output.WriteRaw(text);

                var appMgr = new DomainApplicationClient();
                appMgr.OnGetApplicationRevision += AppMgr_OnGetApplicationRevision;
                appMgr.OnRegisterApplicationRevision += AppMgr_OnRegisterApplicationRevision;

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
                    Applications = appMgr,
                    CommitHash = CommitHash,
                    CommitAuthor = CommitAuthor,
                    CommitMessage = CommitMessage,
                };

                var githubSource = Project?.Source as ProjectGithubSource;
                var notifyGithub = githubSource != null && githubSource.NotifyOrigin == NotifyOrigin.Agent;

                if (notifyGithub && SourceCommit != null)
                    await NotifyGithubStarted(githubSource);

                try {
                    await Domain.RunBuildTask(context, TokenSource.Token);
                }
                catch (Exception error) {
                    Exception = error;
                    throw;
                }
            }
        }

        private void AppMgr_OnGetApplicationRevision(string projectId, string appName, uint deploymentNumber, RemoteTaskCompletionSource<DomainApplicationRevision> taskHandle)
        {
            var app = PhotonAgent.Instance.ApplicationMgr.GetApplication(projectId, appName);
            if (app == null) {
                taskHandle.SetResult(null);
                return;
            }

            var revision = app.GetRevision(deploymentNumber);
            if (revision == null) {
                taskHandle.SetResult(null);
                return;
            }

            var _rev = new DomainApplicationRevision {
                ProjectId = app.ProjectId,
                ApplicationName = app.Name,
                ApplicationPath = revision.Location,
                DeploymentNumber = revision.DeploymentNumber,
                PackageId = revision.PackageId,
                PackageVersion = revision.PackageVersion,
                CreatedTime = revision.Time,
            };

            taskHandle.SetResult(_rev);
        }

        private void AppMgr_OnRegisterApplicationRevision(DomainApplicationRevisionRequest appRevisionRequest, RemoteTaskCompletionSource<DomainApplicationRevision> taskHandle)
        {
            var appMgr = PhotonAgent.Instance.ApplicationMgr;
            var app = appMgr.GetApplication(appRevisionRequest.ProjectId, appRevisionRequest.ApplicationName)
                ?? appMgr.RegisterApplication(appRevisionRequest.ProjectId, appRevisionRequest.ApplicationName);

            var pathName = appRevisionRequest.DeploymentNumber.ToString();

            var revision = new ApplicationRevision {
                DeploymentNumber = appRevisionRequest.DeploymentNumber,
                PackageId = appRevisionRequest.PackageId,
                PackageVersion = appRevisionRequest.PackageVersion,
                Location = NetPath.Combine(app.Location, pathName),
                Time = DateTime.Now,
            };

            app.Revisions.Add(revision);
            appMgr.Save();

            revision.Initialize();

            var _rev = new DomainApplicationRevision {
                ProjectId = app.ProjectId,
                ApplicationName = app.Name,
                ApplicationPath = revision.Location,
                DeploymentNumber = revision.DeploymentNumber,
                PackageId = revision.PackageId,
                PackageVersion = revision.PackageVersion,
                CreatedTime = revision.Time,
            };

            taskHandle.SetResult(_rev);
        }

        public override async Task CompleteAsync()
        {
            var githubSource = Project?.Source as ProjectGithubSource;
            var notifyGithub =  githubSource != null && githubSource.NotifyOrigin == NotifyOrigin.Agent;

            if (notifyGithub && SourceCommit != null)
                await NotifyGithubComplete(githubSource);
        }

        private async Task LoadProjectSource()
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
                    handle = await GetRepositoryHandle(githubSource.CloneUrl, TimeSpan.FromMinutes(1), TokenSource.Token);

                    handle.Username = githubSource.Username;
                    handle.Password = githubSource.Password;
                    handle.UseCommandLine = githubSource.UseCommandLine;
                    handle.CommandLineExe = githubSource.CommandLineExe;
                    handle.EnableTracing = githubSource.EnableTracing;
                    handle.Output = Output;

                    handle.Checkout(GitRefspec, TokenSource.Token);

                    CommitHash = handle.Module?.CommitHash;
                    CommitAuthor = handle.Module?.CommitAuthor;
                    CommitMessage = handle.Module?.CommitMessage;

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

            if (!string.IsNullOrWhiteSpace(PreBuild)) {
                Output.WriteLine("Running Pre-Build Script...", ConsoleColor.DarkCyan);

                try {
                    RunCommandScript(PreBuild);
                }
                catch (Exception error) {
                    Output.WriteLine($"An error occurred while executing the Pre-Build script! {error.Message} [{SessionId}]", ConsoleColor.DarkYellow);
                    throw new ApplicationException($"Script Pre-Build failed! [{SessionId}]", error);
                }
            }

            var assemblyFilename = Path.Combine(ContentDirectory, AssemblyFilename);

            if (!File.Exists(assemblyFilename)) {
                Output.WriteLine($"The assembly file '{assemblyFilename}' could not be found!", ConsoleColor.DarkYellow);
                throw new ApplicationException($"The assembly file '{assemblyFilename}' could not be found!");
            }

            // Shadow-Copy assembly folder
            string assemblyCopyFilename;
            try {
                var sourcePath = Path.GetDirectoryName(assemblyFilename);
                var assemblyName = Path.GetFileName(assemblyFilename);
                assemblyCopyFilename = Path.Combine(BinDirectory, assemblyName);
                CopyDirectory(sourcePath, BinDirectory);
            }
            catch (Exception error) {
                Output.WriteLine($"Failed to shadow-copy assembly '{assemblyFilename}'!", ConsoleColor.DarkYellow);
                throw new ApplicationException($"Failed to shadow-copy assembly '{assemblyFilename}'!", error);
            }

            try {
                Domain = new AgentSessionDomain();
                Domain.Initialize(assemblyCopyFilename);
            }
            catch (Exception error) {
                Output.WriteLine($"An error occurred while initializing the assembly! {error.Message} [{SessionId}]", ConsoleColor.DarkRed);
                throw new ApplicationException($"Failed to initialize Assembly! [{SessionId}]", error);
            }
        }

        private void CopyDirectory(string sourcePath, string destPath)
        {
            var filter = new ContentFilter {
                SourceDirectory = sourcePath,
                DestinationDirectory = destPath,
                DirectoryAction = (src, dest) => Directory.CreateDirectory(dest),
                FileAction = File.Copy,
                IgnoredDirectories = {
                    ".git",
                },
            };

            filter.Run();
        }

        private CommitStatusUpdater GetStatusUpdater(ProjectGithubSource githubSource)
        {
            return new CommitStatusUpdater {
                Username = githubSource.Username,
                Password = githubSource.Password,
                StatusUrl = SourceCommit.StatusesUrl,
                Sha = SourceCommit.Sha,
            };
        }

        private async Task NotifyGithubStarted(ProjectGithubSource githubSource)
        {
            if (SourceCommit == null) {
                Log.Error("Unable to send GitHub notification! Commit is undefined!");
                return;
            }

            var status = new CommitStatus {
                State = CommitStates.Pending,
                Context = "Photon",
                Description = "Build in progress..."
            };

            await GetStatusUpdater(githubSource).Post(status);
        }

        private async Task NotifyGithubComplete(ProjectGithubSource githubSource)
        {
            if (SourceCommit == null) {
                Log.Error("Unable to send GitHub notification! Commit is undefined!");
                return;
            }

            var status = new CommitStatus {
                Context = "Photon",
            };

            if (Exception != null) {
                status.State = CommitStates.Failure;
                status.Description = "Build Failed!";
            }
            else {
                status.State = CommitStates.Success;
                status.Description = "Build Successful.";
            }

            await GetStatusUpdater(githubSource).Post(status);
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
