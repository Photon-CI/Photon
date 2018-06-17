using log4net;
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
            Log.Debug($"Session '{session.SessionId}' processing...");
            TaskResult result;

            try {
                await PreProcessSession(session);

                Log.Debug($"Session '{session.SessionId}' completed successfully.");
                result = TaskResult.Ok();
            }
            catch (OperationCanceledException) {
                Log.Warn($"Session '{session.SessionId}' cancelled.");
                result = TaskResult.Cancel();
            }
            catch (Exception error) {
                Log.Error($"Session '{session.SessionId}' failed!", error);

                session.Exception = error;
                result = TaskResult.Error(error);
            }

            try {
                await PostProcessSession(session, result);
            }
            catch (Exception error) {
                Log.Error($"Session '{session.SessionId}' cleanup failed!", error);
            }
            finally {
                GC.Collect();
            }

            if (result.Successful)
                Log.Debug($"Completed Script Session '{session.SessionId}' successfully.");
            else if (result.Cancelled)
                Log.Warn($"Script Session '{session.SessionId}' was cancelled.");
            else
                Log.Warn($"Completed Script Session '{session.SessionId}' with errors.", session.Exception);
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
            session.Output.WriteLine("Destroying working directory...", ConsoleColor.DarkCyan);

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
