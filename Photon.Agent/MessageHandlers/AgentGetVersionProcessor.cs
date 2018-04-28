using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class AgentGetVersionProcessor : MessageProcessorBase<AgentGetVersionRequest>
    {
        public override async Task<IResponseMessage> Process(AgentGetVersionRequest requestMessage)
        {
            var response = new AgentGetVersionResponse {
                Version = Configuration.Version,
            };

            return await Task.FromResult(response);
        }
    }
}
