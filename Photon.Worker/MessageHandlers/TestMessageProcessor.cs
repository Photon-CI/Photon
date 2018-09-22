using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages.Worker;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Worker.MessageHandlers
{
    internal class TestMessageProcessor : MessageProcessorBase<TestMessageRequest>
    {
        public override Task<IResponseMessage> Process(TestMessageRequest requestMessage)
        {
            var response = new TestMessageResponse {
                Text = new string(requestMessage.Text.Reverse().ToArray()),
            };

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ECHO : {requestMessage.Text}");

            return Task.FromResult<IResponseMessage>(response);
        }
    }
}
