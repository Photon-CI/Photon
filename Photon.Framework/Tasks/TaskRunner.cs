using Photon.Communication;
using Photon.Framework.Pooling;
using Photon.Framework.TcpMessages;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    public class TaskRunner : LifespanReferenceItem
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

            BuildTaskRunResponse startResponse;
            try {
                startResponse = await messageClient.Send(runRequest)
                    .GetResponseAsync<BuildTaskRunResponse>();
            }
            catch (Exception error) {
                throw new Exception($"Failed to run task '{taskName}'! {error.Message}");
            }

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

    public class TaskOutputEventArgs : EventArgs
    {
        public string Text {get;}

        public TaskOutputEventArgs(string text)
        {
            this.Text = text;
        }
    }
}
