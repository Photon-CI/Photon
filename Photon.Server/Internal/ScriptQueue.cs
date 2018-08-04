using log4net;
using Photon.Framework.Extensions;
using Photon.Framework.Tasks;
using Photon.Server.Internal.Sessions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Server.Internal
{
    internal class ScriptQueue : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScriptQueue));

        private readonly CancellationTokenSource tokenSource;
        private ActionBlock<IServerSession> queue;
        private bool isStarted;

        public int MaxDegreeOfParallelism {get; set;}


        public ScriptQueue()
        {
            MaxDegreeOfParallelism = 1;

            tokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            tokenSource?.Dispose();
        }

        public void Start()
        {
            if (isStarted) throw new ApplicationException("ScriptQueue has already been started!");
            isStarted = true;

            Log.Debug($"Starting Script Queue [{MaxDegreeOfParallelism} workers]...");

            var queueOptions = new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                CancellationToken = tokenSource.Token,
            };

            queue = new ActionBlock<IServerSession>(OnProcess, queueOptions);

            Log.Debug("Script Queue started.");
        }

        public void Stop()
        {
            if (!isStarted) return;
            isStarted = false;

            Log.Debug("Stopping Script Queue...");

            queue.Complete();
            queue.Completion.GetAwaiter().GetResult();

            Log.Debug("Script Queue stopped.");
        }

        public void Abort()
        {
            if (!isStarted) return;
            isStarted = false;

            Log.Debug("Aborting Script Queue...");

            tokenSource.Cancel();
            queue.Complete();
            queue.Completion.GetAwaiter().GetResult();

            Log.Debug("Script Queue aborted.");
        }

        public void Add(IServerSession session)
        {
            queue.Post(session);

            Log.Debug($"Queued Script Session '{session.SessionId}'.");
        }

        private async Task OnProcess(IServerSession session)
        {
            Log.Debug($"Session '{session.SessionId}' started...");
            TaskResult result;

            try {
                await PreProcessSession(session);

                if (!session.IsUserAborted) {
                    session.Output.WriteLine("SUCCESS.", ConsoleColor.Green);
                    result = TaskResult.Ok();
                }
                else {
                    session.Output.WriteLine("CANCELLED", ConsoleColor.DarkYellow);
                    result = TaskResult.Cancel();
                }
            }
            catch (Exception error) {
                if (!session.IsUserAborted) {
                    Log.Error($"Session '{session.SessionId}' failed!", error);

                    session.Exception = error;
                    session.Output.WriteBlock(w => w
                        .Write("FAILED! ", ConsoleColor.DarkRed)
                        .WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow));

                    result = TaskResult.Error(error);
                }
                else {
                    session.Output.WriteLine("CANCELLED", ConsoleColor.DarkYellow);
                    result = TaskResult.Cancel();
                }
            }

            try {
                await PostProcessSession(session, result);
            }
            catch (Exception error) {
                Log.Error($"Session '{session.SessionId}' cleanup failed!", error);
            }
            finally {
                Log.Debug($"Session '{session.SessionId}' released.");

                GC.Collect();
            }
        }

        private async Task<TaskResult> PreProcessSession(IServerSession session)
        {
            await session.InitializeAsync();

            try {
                session.OnPreBuildEvent();
            }
            catch (OperationCanceledException) {
                throw;
            }
            catch (Exception error) {
                throw new ApplicationException("Pre-Build event Failed!", error);
            }

            try {
                session.Output.WriteLine("Preparing working directory...", ConsoleColor.DarkCyan);

                await session.PrepareWorkDirectoryAsync();
            }
            catch (OperationCanceledException) {
                throw;
            }
            catch (Exception error) {
                var _e = error;
                if (error is AggregateException _ae)
                    _e = _ae.Flatten();

                throw new ApplicationException("Pre-Build event Failed!", _e);
            }

            try {
                session.Output.WriteLine("Running script...", ConsoleColor.DarkCyan);

                await session.RunAsync();
                return TaskResult.Ok();
            }
            catch (OperationCanceledException) {
                throw;
            }
            catch (Exception error) {
                throw new ApplicationException("Script Failed!", error);
            }
        }

        private async Task PostProcessSession(IServerSession session, TaskResult result)
        {
            try {
                await session.ReleaseAsync();
            }
            catch (Exception error) {
                Log.Warn($"Failed to release session '{session.SessionId}'!", error);
            }
            finally {
                session.Complete(result);
            }

            try {
                session.OnPostBuildEvent();
            }
            catch (Exception error) {
                Log.Error("Session post-build event failed!", error);
            }
        }
    }
}
