using Photon.Framework.Communication;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Photon.Framework.Communication
{
    internal class MessageTransceiver
    {
        private readonly object startStopLock;
        private readonly MessageProcessor processor;
        private readonly ConcurrentDictionary<string, MessageHandle> messageHandles;
        private QueuedMessageSender messageSender;
        private MessageReceiver messageReceiver;
        private NetworkStream stream;
        private bool isStarted;


        public MessageTransceiver(MessageProcessor processor)
        {
            this.processor = processor;

            startStopLock = new object();
            messageHandles = new ConcurrentDictionary<string, MessageHandle>(StringComparer.Ordinal);
            messageSender = new QueuedMessageSender();
            messageReceiver = new MessageReceiver();

            messageReceiver.MessageReceived += MessageReceiver_MessageReceived;
        }

        public void Dispose()
        {
            messageSender?.Dispose();
            messageReceiver.Dispose();
            stream?.Dispose();
        }

        public void Start(NetworkStream stream)
        {
            lock (startStopLock) {
                if (isStarted) throw new Exception("Transceiver is already started!");
                isStarted = true;
            }

            this.stream = stream;

            messageSender.Start(stream);
            messageReceiver.Start(stream);
        }

        public async Task StopAsync()
        {
            lock (startStopLock) {
                if (!isStarted) return;
                isStarted = false;
            }

            await messageSender.StopAsync();
            await messageReceiver.StopAsync();
            //await processor.FlushAsync();

            stream.Close();
        }

        public void SendOneWay(IRequestMessage message)
        {
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

        private void MessageReceiver_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message is IResponseMessage responseMessage) {
                if (!messageHandles.TryGetValue(responseMessage.RequestMessageId, out var handle))
                    throw new Exception($"No request message handle found matching '{responseMessage.RequestMessageId}'!");

                handle.Complete(responseMessage);
            }
            else if (e.Message is IRequestMessage requestMessage) {
                var handle = processor.Process(requestMessage);
                handle.GetResponse().ContinueWith(t => {
                    if (t.IsFaulted) {
                        var exceptionResponse = new ExceptionResponseMessage {
                            RequestMessageId = requestMessage.MessageId,
                            Exception = t.Exception.ToString()
                        };

                        messageSender.Send(exceptionResponse);
                        return;
                    }

                    var _responseMessage = t.Result;
                    _responseMessage.RequestMessageId = requestMessage.MessageId;
                    messageSender.Send(_responseMessage);
                });
            }
            else {
                var messageType = e.Message.GetType();
                throw new Exception($"Unknown message type '{messageType.Name}'!");
            }
        }
    }
}
