using Photon.Agent.Internal.Git;
using Photon.Communication;
using Photon.Framework.Projects;
using Photon.Framework.Tools.Content;
using Photon.Library.Communication.Messages.Session;
using Photon.Library.GitHub;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace Photon.Agent.Internal.Session
{
    internal class AgentBuildSession : AgentSessionBase
    {
        //private readonly TaskCompletionSource<object> completeTask;

        public string PreBuild {get; set;}
        public string GitRefspec {get; set;}
        public string TaskName {get; set;}
        public uint BuildNumber {get; set;}
        public GithubCommit SourceCommit {get; set;}
        public string CommitHash {get; private set;}
        public string CommitAuthor {get; private set;}
        public string CommitMessage {get; private set;}

        //public Task CompletionTask => completeTask.Task;


        public AgentBuildSession(MessageTransceiver serverTransceiver) : base(serverTransceiver)
        {
            //completeTask = new TaskCompletionSource<object>();
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await LoadProjectSource();

            StartWorker();

            var msg = new WorkerBuildSessionRunRequest {
                AgentSessionId = SessionId,
                ServerSessionId = ServerSessionId,
                //SessionClientId = SessionClientId,
                BinDirectory = BinDirectory,
                WorkDirectory = WorkDirectory,
                ContentDirectory = ContentDirectory,
                AssemblyFilename = AssemblyFilename,
                Project = Project,
                Agent = Agent,
            };

            await WorkerHandle.Transceiver.Send(msg)
                .GetResponseAsync(TokenSource.Token);
        }

        public async Task RunTaskAsync()
        {
            try {
                var githubSource = Project?.Source as ProjectGithubSource;
                var notifyGithub = githubSource != null && githubSource.NotifyOrigin == NotifyOrigin.Agent;

                if (notifyGithub && SourceCommit != null)
                    await NotifyGithubStarted(githubSource);
            }
            catch (Exception error) {
                Log.Error("Failed to send GitHub pre-build notification!", error);
            }

            try {
                var request = new WorkerBuildSessionRunRequest {
                    // TODO
                };

                var response = await WorkerHandle.Transceiver.Send(request)
                    .GetResponseAsync();

                //await taskList.AddOrUpdate(taskSessionId, id => task, (id, _) => task);
                //await task.ContinueWith(t => {taskList.TryRemove(taskSessionId, out _);});
            }
            catch (Exception error) {
                Exception = error;
                //completeTask.SetException(error);
                throw;
            }
        }

        //public override async Task CompleteAsync()
        //{
        //    var githubSource = Project?.Source as ProjectGithubSource;
        //    var notifyGithub =  githubSource != null && githubSource.NotifyOrigin == NotifyOrigin.Agent;

        //    if (notifyGithub && SourceCommit != null)
        //        await NotifyGithubComplete(githubSource);
        //}

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

        private async Task<RepositoryHandle> GetRepositoryHandle(string url, TimeSpan timeout, CancellationToken token = default)
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

        private static void CopyDirectory(string sourcePath, string destPath)
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
    }
}
