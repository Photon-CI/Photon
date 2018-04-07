using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Photon.Communication
{
    public class MessageTransceiver
    {
        private readonly object startStopLock;
        private readonly ConcurrentDictionary<string, MessageHandle> messageHandles;
        private readonly MessageSender messageSender;
        private readonly MessageReceiver messageReceiver;
        private TcpClient tcpClient;
        private NetworkStream stream;

        internal MessageProcessor Processor {get;}
        public bool IsStarted {get; private set;}


        internal MessageTransceiver(MessageRegistry registry)
        {
            //this.Processor = processor;
            Processor = new MessageProcessor(this, registry);
            //...

            startStopLock = new object();
            messageHandles = new ConcurrentDictionary<string, MessageHandle>(StringComparer.Ordinal);
            messageSender = new MessageSender();
            messageReceiver = new MessageReceiver();

            messageReceiver.MessageReceived += MessageReceiver_MessageReceived;
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
                        throw new NotImplementedException();
                        //var exceptionResponse = new ExceptionResponseMessage {
                        //    RequestMessageId = requestMessage.MessageId,
                        //    Exception = t.Exception.ToString()
                        //};

                        //messageSender.Send(exceptionResponse);
                        //return;
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
    }
}
