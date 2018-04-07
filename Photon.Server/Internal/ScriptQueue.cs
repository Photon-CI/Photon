using log4net;
using Photon.Server.Internal.Sessions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Server.Internal
{
    internal class ScriptQueue
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScriptQueue));

        private ActionBlock<IServerSession> queue;
        private bool isStarted;

        public int MaxDegreeOfParallelism {get; set;}


        public ScriptQueue()
        {
            MaxDegreeOfParallelism = 1;
        }

        public void Start()
        {
            if (isStarted) throw new ApplicationException("ScriptQueue has already been started!");
            isStarted = true;

            Log.Debug($"Starting Script Queue [{MaxDegreeOfParallelism} workers]...");

            var queueOptions = new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
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

        public void Add(IServerSession session)
        {
            queue.Post(session);

            Log.Debug($"Queued Script Session '{session.SessionId}'.");
        }

        private async Task OnProcess(IServerSession session)
        {
            Log.Debug($"Processing Script Session '{session.SessionId}'.");

            var errorList = new List<Exception>();

            try {
                session.Output.AppendLine("Preparing working directory...");
                session.PrepareWorkDirectory();

                session.Output.AppendLine("Running script...");
                await session.RunAsync();
            }
            catch (Exception error) {
                session.Output.AppendLine($"An unhandled exception occurred!\n{error}");
                errorList.Add(error);
            }

            try {
                await session.ReleaseAsync();
            }
            catch (Exception error) {
                errorList.Add(error);
            }

            if (errorList.Count > 1)
                session.Exception = new AggregateException(errorList);
            else if (errorList.Count == 1)
                session.Exception = errorList[0];

            if (session.Exception != null)
                Log.Warn($"Completed Script Session '{session.SessionId}' with errors.", session.Exception);
            else
                Log.Debug($"Completed Script Session '{session.SessionId}' successfully.");
        }
    }
}
