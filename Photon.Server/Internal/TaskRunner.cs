using Photon.Communication;
using Photon.Framework.Pooling;
using Photon.Library.TcpMessages;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class TaskRunner : LifespanReferenceItem
    {
        public event EventHandler<TaskOutputEventArgs> OutputEvent;

        private readonly string agentSessionId;
        private readonly MessageClient messageClient;
        private readonly StringBuilder output;


        public TaskRunner(MessageClient messageClient, string agentSessionId)
        {
            this.messageClient = messageClient;
            this.agentSessionId = agentSessionId;

            Lifespan = TimeSpan.FromHours(1);
            output = new StringBuilder();
        }

        public async Task Run(string taskName, CancellationToken token)
        {
            var runRequest = new TaskRunRequest {
                AgentSessionId = agentSessionId,
                TaskSessionId = SessionId,
                TaskName = taskName,
            };

            await messageClient.Send(runRequest)
                .GetResponseAsync<TaskRunResponse>(token);
        }

        public void AppendOutput(string text)
        {
            output.Append(text);

            OnOutput(text);
        }

        protected virtual void OnOutput(string text)
        {
            OutputEvent?.Invoke(this, new TaskOutputEventArgs(text));
        }
    }

    public class TaskOutputEventArgs : EventArgs
    {
        public string Text {get;}

        public TaskOutputEventArgs(string text)
        {
            this.Text = text;
        }
    }
}
