using System;
using System.Threading.Tasks;

namespace Photon.Communication
{
    public class MessageProcessorHandle
    {
        private readonly TaskCompletionSource<IResponseMessage> completionEvent;

        //public MessageHost Host {get;}
        public IRequestMessage RequestMessage {get;}


        public MessageProcessorHandle(IRequestMessage requestMessage)
        {
            //this.Host = host;
            this.RequestMessage = requestMessage;

            completionEvent = new TaskCompletionSource<IResponseMessage>();
        }

        public async Task<IResponseMessage> GetResponse()
        {
            return await completionEvent.Task;
        }

        internal void SetResult(IResponseMessage responseMessage)
        {
            completionEvent.SetResult(responseMessage);
        }

        internal void SetException(Exception error)
        {
            completionEvent.SetException(error);
        }
    }
}
