using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class HealthCheckProcessor : MessageProcessorBase<HealthCheckRequest>
    {
        public override async Task<IResponseMessage> Process(HealthCheckRequest requestMessage)
        {
            var response = new HealthCheckResponse {
                AgentVersion = Configuration.Version,
            };

            return await Task.FromResult(response);
        }
    }
}
