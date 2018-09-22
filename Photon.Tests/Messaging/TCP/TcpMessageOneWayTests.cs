using NUnit.Framework;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Tests.Internal;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Tests.Messaging.TCP
{
    [TestFixture, IntegrationTest]
    public class TcpMessageOneWayTests
    {
        private readonly MessageProcessorRegistry registry;


        public TcpMessageOneWayTests()
        {
            registry = new MessageProcessorRegistry();
            registry.Register(typeof(TestMessageProcessor));
        }

        [Test]
        public async Task SendMessageOneWay_ToHost_1000x()
        {
            TestMessageProcessor.Count = 0;

            using (var listener = new MessageListener(registry)) {
                using (var client = new MessageClient(registry)) {
                    listener.Listen(IPAddress.Any, 10934);
                    await client.ConnectAsync("localhost", 10934, CancellationToken.None);

                    for (var i = 0; i < 1000; i++) {
                        client.SendOneWay(new TestRequestOneWayMessage());
                    }

                    client.Disconnect();
                }

                Assert.That(TestMessageProcessor.Count, Is.EqualTo(1_000));

                listener.Stop();
            }
        }

        [Test]
        public async Task SendMessageOneWay_FromHost_1000x()
        {
            TestMessageProcessor.Count = 0;

            using (var listener = new MessageListener(registry)) {
                using (var client = new MessageClient(registry)) {
                    listener.ConnectionReceived += (sender, e) => {
                        for (var i = 0; i < 1000; i++) {
                            e.Host.SendOneWay(new TestRequestOneWayMessage());
                        }

                        e.Host.Stop();
                    };

                    listener.Listen(IPAddress.Loopback, 10934);
                    await client.ConnectAsync("localhost", 10934, CancellationToken.None);

                    await Task.Delay(200);
                    client.Disconnect();
                }

                Assert.That(TestMessageProcessor.Count, Is.EqualTo(1_000));

                listener.Stop();
            }
        }

        private class TestRequestOneWayMessage : IRequestMessage
        {
            public string MessageId {get; set;}
        }

        private class TestMessageProcessor : MessageProcessorBase<TestRequestOneWayMessage>
        {
            public static int Count;

            public override async Task<IResponseMessage> Process(TestRequestOneWayMessage requestMessage)
            {
                Interlocked.Increment(ref Count);

                return await Task.FromResult<IResponseMessage>(null);
            }
        }
    }
}
