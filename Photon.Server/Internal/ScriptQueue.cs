using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Server.Internal
{
    internal class ScriptQueue
    {
        private ActionBlock<IServerSession> queue;

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

            queue = new ActionBlock<IServerSession>(OnProcess, queueOptions);
        }

        public void Stop()
        {
            queue.Complete();
            queue.Completion.GetAwaiter().GetResult();
        }

        public void Add(IServerSession session)
        {
            queue.Post(session);
        }

        private async Task OnProcess(IServerSession session)
        {
            var errorList = new List<Exception>();

            try {
                try {
                    session.Run();
                }
                catch (Exception error) {
                    errorList.Add(error);
                }
            }
            catch (Exception error) {
                errorList.Add(error);
            }

            try {
                session.Release();
            }
            catch (Exception error) {
                errorList.Add(error);
            }

            if (errorList.Count > 1)
                session.Exception = new AggregateException(errorList);
            else if (errorList.Count == 1)
                session.Exception = errorList[0];
        }
    }
}
