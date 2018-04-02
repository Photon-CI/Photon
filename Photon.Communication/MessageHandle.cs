using System.Threading.Tasks;

namespace Photon.Communication
{
    public class MessageHandle
    {
        private TaskCompletionSource<IResponseMessage> completionEvent;

        public IRequestMessage Request {get;}
        public IResponseMessage Response {get; private set;}


        public MessageHandle(IRequestMessage request)
        {
            this.Request = request;

            completionEvent = new TaskCompletionSource<IResponseMessage>();
        }

        public async Task<IResponseMessage> GetResponseAsync()
        {
            return await completionEvent.Task;
        }

        public async Task<T> GetResponseAsync<T>()
        {
            return (T)await completionEvent.Task;
        }

        internal void Complete(IResponseMessage message)
        {
            this.Response = message;

            completionEvent.SetResult(message);
        }
    }
}
