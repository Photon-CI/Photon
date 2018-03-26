using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Server.Internal
{
    internal class ScriptQueue
    {
        private ActionBlock<ServerSession> queue;

        public int MaxDegreeOfParallelism {get; set;}


        public ScriptQueue()
        {
            MaxDegreeOfParallelism = 1;
        }

        public void Start()
        {
            var queueOptions = new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
            };

            queue = new ActionBlock<ServerSession>(OnProcess, queueOptions);
        }

        public void Stop()
        {
            queue.Complete();
            queue.Completion.GetAwaiter().GetResult();
        }

        public void Add(ServerSession session)
        {
            queue.Post(session);
        }

        private async Task OnProcess(ServerSession session)
        {
            var errorList = new List<Exception>();

            try {
                await session.InitializeAsync();

                try {
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
                await session.CleanupAsync();
            }
            catch (Exception error) {
                errorList.Add(error);
            }

            if (errorList.Count > 1)
                session.Exception = new AggregateException(errorList);
            else if (errorList.Count == 1)
                session.Exception = errorList[0];

            session.Release();
        }
    }
}
