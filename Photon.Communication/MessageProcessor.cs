using Photon.Communication.Messages;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Communication
{
    internal class MessageProcessor
    {
        private readonly MessageProcessorRegistry registry;
        private readonly MessageTransceiver transceiver;

        private ActionBlock<MessageProcessorHandle> queue;

        public object Context {get; set;}


        public MessageProcessor(MessageTransceiver transceiver, MessageProcessorRegistry registry)
        {
            this.transceiver = transceiver;
            this.registry = registry;
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

        public MessageProcessorHandle Process(IRequestMessage requestMessage)
        {
            var handle = new MessageProcessorHandle(requestMessage);
            queue.Post(handle);
            return handle;
        }

        private async Task OnProcess(MessageProcessorHandle handle)
        {
            try {
                var messageType = handle.RequestMessage.GetType();
                if (!registry.TryGet(messageType, out var processFunc))
                    throw new ApplicationException($"No Message Processor was found matching message type '{messageType.Name}'!");

                var result = await processFunc(transceiver, handle.RequestMessage);

                handle.SetResult(result);
            }
            catch (Exception error) {
                handle.SetException(error);
            }
        }
    }
}
