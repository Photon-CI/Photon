using NUnit.Framework;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Tests.Internal;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Tests.Messaging
{
    [TestFixture, IntegrationTest]
    public class DisconnectTests
    {
        private const string Host = "localhost";
        private const int Port = 10931;


        [Test]
        public async Task ClientDisconnectWaitsForMessages()
        {
            var registry = new MessageProcessorRegistry();
            registry.Register(typeof(DelayedTestProcessor));

            using (var listener = new MessageListener(registry))
            using (var client = new MessageClient(registry)) {
                listener.Listen(IPAddress.Loopback, Port);
                await client.ConnectAsync(Host, Port, CancellationToken.None);

                DelayedTestProcessor.Complete = false;
                var message = new DelayedTestRequest();
                var _ = client.Send(message).GetResponseAsync<DelayedTestResponse>();

                client.Disconnect();
                //await task;

                Assert.That(DelayedTestProcessor.Complete, Is.True);

                listener.Stop();
            }
        }

        private class DelayedTestRequest : IRequestMessage
        {
            public string MessageId {get; set;}
        }

        private class DelayedTestResponse : ResponseMessageBase {}

        private class DelayedTestProcessor : MessageProcessorBase<DelayedTestRequest>
        {
            public static bool Complete {get; set;}


            public override async Task<IResponseMessage> Process(DelayedTestRequest requestMessage)
            {
                await Task.Delay(800);

                Complete = true;
                return new DelayedTestResponse();
            }
        }
    }
}
