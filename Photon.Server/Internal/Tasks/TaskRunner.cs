using Photon.Communication;
using Photon.Framework.Tasks;
using Photon.Library;
using Photon.Library.Messages;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Tasks
{
    internal class TaskRunner : LifespanReferenceItem
    {
        public event EventHandler<TaskOutputEventArgs> OutputEvent;

        private readonly MessageClient messageClient;
        private readonly StringBuilder output;

        private readonly string agentSessionId;


        public TaskRunner(MessageClient messageClient, string agentSessionId)
        {
            this.messageClient = messageClient;
            this.agentSessionId = agentSessionId;

            Lifespan = TimeSpan.FromHours(1);
            output = new StringBuilder();
        }

        public async Task<TaskResult> Run(string taskName)
        {
            var runRequest = new BuildTaskRunRequest {
                AgentSessionId = agentSessionId,
                TaskSessionId = SessionId,
                TaskName = taskName,
            };

            var startResponse = await messageClient.Send(runRequest)
                .GetResponseAsync<BuildTaskRunResponse>();

            if (!startResponse.Successful)
                throw new Exception($"Failed to run task '{taskName}'! {startResponse.Exception}");

            return startResponse.Result;
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

    internal class TaskOutputEventArgs : EventArgs
    {
        public string Text {get;}

        public TaskOutputEventArgs(string text)
        {
            this.Text = text;
        }
    }
}
