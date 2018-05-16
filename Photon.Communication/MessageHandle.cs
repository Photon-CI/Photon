using Photon.Communication.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Communication
{
    public class MessageHandle
    {
        private readonly TaskCompletionSource<IResponseMessage> completionEvent;
        private bool isComplete;

        public IRequestMessage Request {get;}
        public IResponseMessage Response {get; private set;}


        public MessageHandle(IRequestMessage request)
        {
            this.Request = request;

            isComplete = false;
            completionEvent = new TaskCompletionSource<IResponseMessage>();
        }

        public async Task<IResponseMessage> GetResponseAsync(CancellationToken token = default(CancellationToken))
        {
            token.Register(() => {
                if (isComplete) return;
                isComplete = true;
                
                completionEvent.SetCanceled();
            });

            var response = await completionEvent.Task;

            if (!(response?.Successful ?? false))
                throw new Exception(response?.ExceptionMessage ?? "An unknown error occurred!");

            return response;
        }

        public async Task<T> GetResponseAsync<T>(CancellationToken token)
            where T : class, IResponseMessage
        {
            token.Register(() => completionEvent.SetCanceled());

            var response = await completionEvent.Task;

            if (!(response?.Successful ?? false))
                throw new Exception(response?.ExceptionMessage ?? "An unknown error occurred!");

            if (!(response is T tResponse))
                throw new Exception($"Unable to cast response type '{response.GetType().Name}' to '{typeof(T).Name}'!");

            return tResponse;
        }

        public async Task<T> GetResponseAsync<T>()
            where T : class, IResponseMessage
        {
            return await GetResponseAsync<T>(CancellationToken.None);
        }

        internal void Complete(IResponseMessage message)
        {
            this.Response = message;

            if (isComplete) return;
            isComplete = true;

            completionEvent.SetResult(message);
        }
    }
}
