using Photon.Communication;
using Photon.Framework.Scripts;
using System;
using Photon.Framework.TcpMessages;

namespace Photon.Framework.Sessions
{
    public interface ISessionOutput
    {
        ISessionOutput Write(string text, ConsoleColor color = ConsoleColor.Gray);
        ISessionOutput WriteLine(string text, ConsoleColor color = ConsoleColor.Gray);
    }

    [Serializable]
    public class SessionOutput : ISessionOutput
    {
        private readonly string serverSessionId;
        private readonly MessageTransceiver transceiver;
        private readonly ScriptOutput output;
        private int readPos;

        public ScriptOutput Writer => output;


        public SessionOutput(MessageTransceiver transceiver, string serverSessionId)
        {
            this.transceiver = transceiver;
            this.serverSessionId = serverSessionId;

            output = new ScriptOutput();
            output.Changed += Output_OnChanged;
        }

        public ISessionOutput Write(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            output.Append(text, color);
            return this;
        }

        public ISessionOutput WriteLine(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            output.AppendLine(text, color);
            return this;
        }

        private void Output_OnChanged(object sender, EventArgs e)
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
