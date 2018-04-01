using log4net;
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

        public int MaxDegreeOfParallelism {get; set;}


        public ScriptQueue()
        {
            MaxDegreeOfParallelism = 1;
        }

        public void Start()
        {
            Log.Debug($"Starting Script Queue [{MaxDegreeOfParallelism} workers]...");

            var queueOptions = new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
            };

            queue = new ActionBlock<IServerSession>(OnProcess, queueOptions);

            Log.Debug("Script Queue started.");
        }

        public void Stop()
        {
            Log.Debug("Stopping Script Queue...");

            queue.Complete();
            queue.Completion.GetAwaiter().GetResult();

            Log.Debug("Script Queue stopped.");
        }

        public void Add(IServerSession session)
        {
            queue.Post(session);

            Log.Debug($"Queued Script Session '{session.Id}'.");
        }

        private async Task OnProcess(IServerSession session)
        {
            Log.Debug($"Processing Script Session '{session.Id}'.");

            var errorList = new List<Exception>();

            try {
                try {
                    session.PrepareWorkDirectory();

                    await session.RunAsync();
                }
                catch (Exception error) {
                    errorList.Add(error);
                }
            }
            catch (Exception error) {
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
                Log.Warn($"Completed Script Session '{session.Id}' with errors.", session.Exception);
            else
                Log.Debug($"Completed Script Session '{session.Id}' successfully.");
        }
    }
}
