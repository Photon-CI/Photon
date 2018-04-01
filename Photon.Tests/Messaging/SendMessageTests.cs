using NUnit.Framework;
using Photon.Framework.Communication;
using Photon.Tests.Internal;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Photon.Tests
{
    public class SendMessageTests : IntegrationTestFixture, IDisposable
    {
        private const int port = 10933;

        private readonly MessageProcessor processor;
        private readonly MessageListener listener;
        private readonly MessageClient client;


        public SendMessageTests()
        {
            processor = new MessageProcessor();
            processor.Register(typeof(TestMessageProcessor));
            processor.Register(typeof(TestMessageOneWayProcessor));

            listener = new MessageListener(processor);
            client = new MessageClient(processor);
        }

        public void Dispose()
        {
            client.Dispose();
            //listener.Dispose();
            //processor.Dispose();
        }

        [OneTimeSetUp]
        public async Task Begin()
        {
            processor.Start();
            listener.Listen(IPAddress.Any, port);

            await client.ConnectAsync("localhost", port);
        }

        [OneTimeTearDown]
        public async Task End()
        {
            await client.DisconnectAsync();
            await listener.StopAsync();
            await processor.StopAsync();
        }

        [Test]
        public async Task SendMessageOneWay()
        {
            var completeEvent = new TaskCompletionSource<bool>();
            TestMessageOneWayProcessor.Event = completeEvent;

            var message = new TestRequestOneWayMessage();
            client.SendOneWay(message);

            var result = await completeEvent.Task;
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task SendMessageResponse()
        {
            var request = new TestRequestMessage {
                Value = 2,
            };

            var response = await client.Send(request)
                .GetResponseAsync<TestResponseMessage>();

            Assert.That(response.Value, Is.EqualTo(4));
        }

        class TestRequestOneWayMessage : IRequestMessage
        {
            public string MessageId {get; set;}
            public int Value {get; set;}
        }

        class TestRequestMessage : IRequestMessage
        {
            public string MessageId {get; set;}
            public int Value {get; set;}
        }

        class TestResponseMessage : IResponseMessage
        {
            public string MessageId {get; set;}
            public string RequestMessageId {get; set;}
            public int Value {get; set;}
        }

        class TestMessageProcessor : IProcessMessage<TestRequestMessage>
        {
            public async Task<IResponseMessage> Process(TestRequestMessage requestMessage)
            {
                return await Task.FromResult(new TestResponseMessage {
                    RequestMessageId = requestMessage.MessageId,
                    Value = requestMessage.Value * 2,
                });
            }
        }

        class TestMessageOneWayProcessor : IProcessMessage<TestRequestOneWayMessage>
        {
            public static TaskCompletionSource<bool> Event {get; set;}

            public async Task<IResponseMessage> Process(TestRequestOneWayMessage requestMessage)
            {
                Event?.SetResult(true);

                return await Task.FromResult((IResponseMessage)null);
            }
        }
    }
}
