using Photon.Communication.Messages;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Photon.Communication
{
    public class MessageTransceiver
    {
        public event UnhandledExceptionEventHandler ThreadException;

        private readonly object startStopLock;
        private readonly ConcurrentDictionary<string, MessageHandle> messageHandles;
        private readonly MessageSender messageSender;
        private readonly MessageReceiver messageReceiver;
        private TcpClient tcpClient;
        private NetworkStream stream;

        internal MessageProcessor Processor {get;}
        public object Context {get; set;}
        public bool IsStarted {get; private set;}


        internal MessageTransceiver(MessageProcessorRegistry registry)
        {
            startStopLock = new object();
            messageHandles = new ConcurrentDictionary<string, MessageHandle>(StringComparer.Ordinal);
            messageSender = new MessageSender();
            Processor = new MessageProcessor(this, registry);
            messageReceiver = new MessageReceiver();

            messageReceiver.MessageReceived += MessageReceiver_MessageReceived;
            messageReceiver.ThreadException += MessageReceiver_OnThreadException;
        }

        public void Dispose()
        {
            messageSender?.Dispose();
            messageReceiver.Dispose();
            stream?.Dispose();
            tcpClient?.Dispose();
        }

        public void Start(TcpClient client)
        {
            lock (startStopLock) {
                if (IsStarted) throw new Exception("Transceiver is already started!");
                IsStarted = true;
            }

            tcpClient = client;
            stream = tcpClient.GetStream();

            Processor.Start();
            messageSender.Start(stream);
            messageReceiver.Start(stream);
        }

        public async Task StopAsync()
        {
            lock (startStopLock) {
                if (!IsStarted) return;
                IsStarted = false;
            }

            await Processor.StopAsync();
            await messageSender.StopAsync();
            await messageReceiver.StopAsync();

            stream.Close();
        }

        public void SendOneWay(IRequestMessage message)
        {
            message.MessageId = Guid.NewGuid().ToString("N");

            messageSender.Send(message);
        }

        public MessageHandle Send(IRequestMessage message)
        {
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
                if (!messageHandles.TryGetValue(responseMessage.RequestMessageId, out var handle))
                    throw new Exception($"No request message handle found matching '{responseMessage.RequestMessageId}'!");

                handle.Complete(responseMessage);
            }
            else if (e.Message is IRequestMessage requestMessage) {
                var handle = Processor.Process(requestMessage);
                handle.GetResponse().ContinueWith(t => {
                    if (t.IsFaulted) {
                        var exceptionResponse = new ExceptionResponseMessage {
                            MessageId = Guid.NewGuid().ToString("N"),
                            RequestMessageId = requestMessage.MessageId,
                            Exception = t.Exception?.Message,
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
                throw new Exception($"Unknown message type '{messageType.Name}'!");
            }
        }

        private void MessageReceiver_OnThreadException(object sender, UnhandledExceptionEventArgs e)
        {
            OnThreadException(e.ExceptionObject);
        }
    }
}
