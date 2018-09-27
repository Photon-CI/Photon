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
    internal class SessionQueue : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SessionQueue));

        private readonly CancellationTokenSource tokenSource;
        private ActionBlock<IServerSession> queue;
        private bool isStarted;

        public int MaxDegreeOfParallelism {get; set;}


        public SessionQueue()
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
            TaskResult result = null;

            try {
                await session.InitializeAsync();

                await session.RunAsync();

                session.Output.WriteLine("SUCCESS.", ConsoleColor.Green);
                result = TaskResult.Ok();
            }
            catch (Exception error) {
                if (session.IsUserAborted) {
                    session.Output.WriteLine("CANCELLED", ConsoleColor.DarkYellow);
                    result = TaskResult.Cancel();
                }
                else {
                    Log.Error($"Session '{session.SessionId}' failed!", error);

                    session.Exception = error;
                    session.Output.WriteBlock(w => w
                        .Write("FAILED! ", ConsoleColor.DarkRed)
                        .WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow));

                    result = TaskResult.Error(error);
                }
            }
            finally {
                session.Complete(result);

                try {
                    session.Release();
                }
                catch (Exception error) {
                    Log.Error($"Failed to release session '{session.SessionId}'!", error);
                }

                Log.Debug($"Session '{session.SessionId}' released.");
                GC.Collect();
            }
        }
    }
}
