using Photon.Communication;
using Photon.Framework.Server;
using Photon.Library.TcpMessages;
using System;

namespace Photon.Agent.Internal.Session
{
    /// <summary>
    /// Wraps a <see cref="ScriptOutput"/> object
    /// and sends updates to a TCP host.
    /// </summary>
    public class SessionOutput
    {
        private readonly string serverSessionId;
        private readonly string sessionClientId;
        private readonly MessageTransceiver transceiver;
        private int readPos;

        public ScriptOutput Writer {get;}

        public int Length => Writer.Length;


        public SessionOutput(MessageTransceiver transceiver, string serverSessionId, string sessionClientId)
        {
            this.transceiver = transceiver;
            this.serverSessionId = serverSessionId;
            this.sessionClientId = sessionClientId;

            Writer = new ScriptOutput();
            Writer.Changed += Output_OnChanged;
        }

        public void Close()
        {
            Writer.Changed -= Output_OnChanged;
        }

        public SessionOutput Write(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            Writer.Append(text, color);
            return this;
        }

        public SessionOutput WriteLine(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            Writer.AppendLine(text, color);
            return this;
        }

        private void Output_OnChanged(object sender, EventArgs e)
        {
            var length = Writer.Length;
            if (length <= readPos) return;

            var newText = Writer.GetString().Substring(readPos);
            readPos = length;

            var message = new SessionOutputMessage {
                ServerSessionId = serverSessionId,
                SessionClientId = sessionClientId,
                Text = newText,
            };

            transceiver.SendOneWay(message);
        }
    }
}
