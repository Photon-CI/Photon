using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class AgentGetVersionProcessor : MessageProcessorBase<AgentGetVersionRequest>
    {
        public override async Task<IResponseMessage> Process(AgentGetVersionRequest requestMessage)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            var response = new AgentGetVersionResponse {
                Version = assemblyInfo.ProductVersion,
            };

            return await Task.FromResult(response);
        }
    }
}
