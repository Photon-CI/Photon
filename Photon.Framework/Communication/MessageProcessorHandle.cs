using Photon.Framework.Communication;
using System.Threading.Tasks;

namespace Photon.Framework.Communication
{
    public class MessageProcessorHandle
    {
        private readonly TaskCompletionSource<IResponseMessage> completionEvent;

        public IRequestMessage RequestMessage {get;}


        public MessageProcessorHandle(IRequestMessage requestMessage)
        {
            this.RequestMessage = requestMessage;

            completionEvent = new TaskCompletionSource<IResponseMessage>();
        }

        public async Task<IResponseMessage> GetResponse()
        {
            return await completionEvent.Task;
        }

        internal void Complete(IResponseMessage responseMessage)
        {
            completionEvent.SetResult(responseMessage);
        }
    }
}
