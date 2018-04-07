using Photon.Communication;
using Photon.Framework.Messages;
using Photon.Framework.Scripts;
using System;

namespace Photon.Framework.Tasks
{
    public interface ITaskOutput
    {
        ITaskOutput Write(string text, ConsoleColor color = ConsoleColor.Gray);
        ITaskOutput WriteLine(string text, ConsoleColor color = ConsoleColor.Gray);
    }

    public class TaskOutput : ITaskOutput
    {
        private readonly string taskSessionId;
        private readonly MessageTransceiver transceiver;
        private readonly ScriptOutput output;
        private int readPos;


        public TaskOutput(MessageTransceiver transceiver, string taskSessionId)
        {
            this.transceiver = transceiver;
            this.taskSessionId = taskSessionId;

            output = new ScriptOutput();
        }

        public ITaskOutput Write(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            output.Append(text, color);
            Update();
            return this;
        }

        public ITaskOutput WriteLine(string text, ConsoleColor color = ConsoleColor.Gray)
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

            var message = new BuildTaskOutputMessage {
                TaskSessionId = taskSessionId,
                Text = newText,
            };

            transceiver.SendOneWay(message);
        }
    }
}
