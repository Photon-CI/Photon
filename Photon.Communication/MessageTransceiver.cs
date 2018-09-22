using Photon.Communication.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Communication
{
    public class MessageTransceiver : IDisposable
    {
        public event UnhandledExceptionEventHandler ThreadException;

        private readonly object startStopLock;
        private readonly ConcurrentDictionary<string, MessageHandle> messageHandles;
        private readonly MessageSender messageSender;
        private readonly MessageReceiver messageReceiver;
        private Stream stream;

        internal MessageProcessor Processor {get;}
        public object Context {get; set;}
        public bool IsStarted {get; private set;}


        public MessageTransceiver(MessageProcessorRegistry registry)
        {
            startStopLock = new object();
            messageHandles = new ConcurrentDictionary<string, MessageHandle>(StringComparer.Ordinal);
            messageSender = new MessageSender();
            Processor = new MessageProcessor(this, registry);
            messageReceiver = new MessageReceiver();

            messageSender.ThreadError += MessageSender_OnThreadError;
            messageReceiver.MessageReceived += MessageReceiver_MessageReceived;
            //messageReceiver.ThreadException += MessageReceiver_OnThreadException;
        }

        public void Dispose()
        {
            messageSender?.Dispose();
            messageReceiver.Dispose();
            stream?.Dispose();
        }

        public void Start(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead) throw new ArgumentException("Stream must be readable!", nameof(stream));
            if (!stream.CanWrite) throw new ArgumentException("Stream must be writable!", nameof(stream));

            lock (startStopLock) {
                if (IsStarted) throw new Exception("Transceiver is already started!");
                IsStarted = true;
            }

            this.stream = stream;

            Processor.Start();
            messageSender.Start(stream);
            messageReceiver.Start(stream);
        }

        public void Flush(CancellationToken cancellationToken = default(CancellationToken))
        {
            messageReceiver.Flush(cancellationToken);
            Processor.Flush(cancellationToken);
            messageSender.Flush(cancellationToken);
            stream.Flush();
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            messageReceiver.Flush(cancellationToken);
            await Processor.FlushAsync(cancellationToken);
            await messageSender.FlushAsync(cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        public void Stop()
        {
            lock (startStopLock) {
                if (!IsStarted) return;
                IsStarted = false;
            }

            try {
                messageSender.Stop();
            }
            catch {}

            try {
                messageReceiver.Stop();
            }
            catch {}
        }

        public void SendOneWay(IRequestMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.MessageId = Guid.NewGuid().ToString("N");

            messageSender.Send(message);
        }

        public MessageHandle Send(IRequestMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.MessageId = Guid.NewGuid().ToString("N");

            var handle = new MessageHandle(message);
            messageHandles[message.MessageId] = handle;
            messageSender.Send(message);

            return handle;
        }

        protected virtual void OnThreadException(object exceptionObject)
        {
            ThreadException?.Invoke(this, new UnhandledExceptionEventArgs(exceptionObject, false));
        }

        private void MessageReceiver_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message is IResponseMessage responseMessage) {
                if (!messageHandles.TryGetValue(responseMessage.RequestMessageId, out var handle)) {
                    OnThreadException(new Exception($"No request message handle found matching '{responseMessage.RequestMessageId}'!"));
                    return;
                }

                handle.Complete(responseMessage);
            }
            else if (e.Message is IRequestMessage requestMessage) {
                var handle = Processor.Process(requestMessage);

                handle.GetResponse().ContinueWith(t => {
                    if (t.IsFaulted) {
                        var error = t.Exception?.Flatten();

                        var exceptionResponse = new ResponseMessageBase {
                            MessageId = Guid.NewGuid().ToString("N"),
                            RequestMessageId = requestMessage.MessageId,
                            ExceptionMessage = UnfoldMessages(error),
                            Exception = error?.ToString(),
                            Successful = false,
                        };

                        messageSender.Send(exceptionResponse);
                        return;
                    }

                    var _responseMessage = t.Result;
                    if (_responseMessage != null) {
                        _responseMessage.MessageId = Guid.NewGuid().ToString("N");
                        _responseMessage.RequestMessageId = requestMessage.MessageId;
                        messageSender.Send(_responseMessage);
                    }
                });
            }
            else {
                var messageType = e.Message.GetType();
                OnThreadException(new Exception($"Unknown message type '{messageType.Name}'!"));
            }
        }

        //private void MessageReceiver_OnThreadException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    OnThreadException(e.ExceptionObject);
        //}

        private void MessageSender_OnThreadError(object sender, ThreadExceptionEventArgs e)
        {
            OnThreadException(e.Exception);
        }

        private static string UnfoldMessages(Exception error)
        {
            return string.Join(" ", UnfoldExceptions(error).Select(e => e.Message));
        }

        private static IEnumerable<Exception> UnfoldExceptions(Exception error)
        {
            var e = error;
            while (e != null) {
                yield return e;
                e = e.InnerException;
            }
        }
    }
}
