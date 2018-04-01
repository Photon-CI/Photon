﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Framework.Communication
{
    public class MessageProcessor
    {
        private readonly MessageRegistry registry;
        private ActionBlock<MessageProcessorHandle> queue;


        public MessageProcessor()
        {
            registry = new MessageRegistry();
        }

        public void Scan(Assembly assembly)
        {
            registry.Scan(assembly);
        }

        public void Start()
        {
            queue = new ActionBlock<MessageProcessorHandle>(OnProcess);
        }

        public async Task StopAsync()
        {
            queue.Complete();
            await queue.Completion;
        }

        public void Register(Type processorClassType)
        {
            registry.Register(processorClassType);
        }

        public void Register<TProcessor, TRequest>()
            where TProcessor : IProcessMessage<TRequest>
            where TRequest : IRequestMessage
        {
            registry.Register<TProcessor, TRequest>();
        }

        public MessageProcessorHandle Process(IRequestMessage requestMessage)
        {
            var handle = new MessageProcessorHandle(requestMessage);
            queue.Post(handle);
            return handle;
        }

        //public async Task FlushAsync()
        //{
        //    //...
        //    throw new NotImplementedException();
        //}

        private async Task OnProcess(MessageProcessorHandle handle)
        {
            try {
                var result = await registry.Process(handle.RequestMessage);

                handle.SetResult(result);
            }
            catch (Exception error) {
                handle.SetException(error);
            }
        }
    }
}