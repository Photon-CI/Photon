using Photon.Communication;
using Photon.Library;
using System.Threading.Tasks;
using Photon.Framework.Tasks;
using Photon.Library.TcpMessages;

namespace Photon.Agent.Internal.Session
{
    internal class SessionTaskHandle : LifespanReferenceItem
    {
        private readonly AgentSessionDomain domain;
        private readonly MessageTransceiver transceiver;
        private Task task;


        public SessionTaskHandle(AgentSessionDomain domain, MessageTransceiver transceiver)
        {
            this.domain = domain;
            this.transceiver = transceiver;
        }

        public void Begin(AgentBuildContext context)
        {
            task = domain.RunBuildTask(context);
            task.ContinueWith(t => {
                var completeMessage = new BuildTaskCompleteMessage {
                    TaskId = SessionId,
                    Text = "Complete!",
                };

                transceiver.SendOneWay(completeMessage);
            });
        }
    }
}
