using Photon.Framework.Communication;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Framework.Communication
{
    public class MessageProcessor
    {
        private readonly MessageProcessorRegistry registry;
        private ActionBlock<MessageProcessorHandle> queue;


        public MessageProcessor()
        {
            registry = new MessageProcessorRegistry();
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
            var result = await registry.Process(handle.RequestMessage);
            handle.Complete(result);
        }
    }
}
