using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Server;
using Photon.Library.TcpMessages;
using System;

namespace Photon.Worker.Internal.Session
{
    /// <summary>
    /// Wraps a <see cref="ScriptOutput"/> object
    /// and sends updates to a TCP host.
    /// </summary>
    public class SessionOutput : IWriteBlocks<SessionOutput, SessionBlockWriter>, IDisposable
    {
        private readonly string serverSessionId;
        private readonly string agentSessionId;
        //private readonly string sessionClientId;
        private readonly MessageTransceiver transceiver;
        private int readPos;
        private bool isClosed;

        public ScriptOutput Writer {get;}

        public int Length => Writer.Length;


        public SessionOutput(MessageTransceiver transceiver, string serverSessionId, string agentSessionId)
        {
            this.transceiver = transceiver;
            this.serverSessionId = serverSessionId;
            //this.sessionClientId = sessionClientId;
            this.agentSessionId = agentSessionId;

            Writer = new ScriptOutput();
            Writer.Changed += Output_OnChanged;
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (isClosed) return;
            isClosed = true;

            Writer.Changed -= Output_OnChanged;
        }

        public SessionOutput Write(string text, ConsoleColor color)
        {
            Writer.Write(text, color);
            return this;
        }

        public SessionOutput Write(object value, ConsoleColor color)
        {
            Writer.Write(value, color);
            return this;
        }

        public SessionOutput WriteLine(string text, ConsoleColor color)
        {
            Writer.WriteLine(text, color);
            return this;
        }

        public SessionOutput WriteLine(object value, ConsoleColor color)
        {
            Writer.WriteLine(value, color);
            return this;
        }

        public SessionOutput WriteRaw(string text)
        {
            Writer.WriteRaw(text);
            return this;
        }

        IBlockWriter IWriteBlocks.WriteBlock()
        {
            return new SessionBlockWriter(this);
        }

        public SessionBlockWriter WriteBlock()
        {
            return new SessionBlockWriter(this);
        }

        private void Output_OnChanged(object sender, EventArgs e)
        {
            var length = Writer.Length;
            if (length <= readPos) return;

            var newText = Writer.GetString().Substring(readPos);
            readPos = length;

            var message = new SessionOutputMessage {
                ServerSessionId = serverSessionId,
                AgentSessionId = agentSessionId,
                //SessionClientId = sessionClientId,
                Text = newText,
            };

            transceiver.SendOneWay(message);
        }

        void IWrite.Write(string text, ConsoleColor color) => Write(text, color);
        void IWrite.Write(object value, ConsoleColor color) => Write(value, color);
        void IWrite.WriteLine(string text, ConsoleColor color) => WriteLine(text, color);
        void IWrite.WriteLine(object value, ConsoleColor color) => WriteLine(value, color);
    }
}
