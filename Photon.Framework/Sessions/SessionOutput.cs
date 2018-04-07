using Photon.Communication;
using Photon.Framework.Messages;
using Photon.Framework.Scripts;
using System;

namespace Photon.Framework.Sessions
{
    public interface ISessionOutput
    {
        ISessionOutput Write(string text, ConsoleColor color = ConsoleColor.Gray);
        ISessionOutput WriteLine(string text, ConsoleColor color = ConsoleColor.Gray);
    }

    public class SessionOutput : ISessionOutput
    {
        private readonly string serverSessionId;
        private readonly MessageTransceiver transceiver;
        private readonly ScriptOutput output;
        private int readPos;


        public SessionOutput(MessageTransceiver transceiver, string serverSessionId)
        {
            this.transceiver = transceiver;
            this.serverSessionId = serverSessionId;

            output = new ScriptOutput();
        }

        public ISessionOutput Write(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            output.Append(text, color);
            Update();
            return this;
        }

        public ISessionOutput WriteLine(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            output.AppendLine(text, color);
            Update();
            return this;
        }

        private void Update()
        {
            var length = output.Length;
            if (length <= readPos) return;

            var newText = output.ToString().Substring(readPos);
            readPos = length;

            var message = new AgentSessionOutputMessage {
                ServerSessionId = serverSessionId,
                Text = newText,
            };

            transceiver.SendOneWay(message);
        }
    }
}
