using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SysProcess = System.Diagnostics.Process;

namespace Photon.Agent.MessageHandlers
{
    public class AgentUpdateProcessor : MessageProcessorBase<AgentUpdateRequest>
    {
        public override async Task<IResponseMessage> Process(AgentUpdateRequest requestMessage)
        {
            var msiFilename = Path.Combine(Configuration.Directory, "Photon.Agent.Installer.msi");

            if (File.Exists(msiFilename))
                File.Delete(msiFilename);

            File.Move(requestMessage.Filename, msiFilename);

            Run(msiFilename);

            var response = new AgentUpdateResponse();

            return await Task.FromResult(response);
        }

        private static void Run(string msiFilename)
        {
            var info = new ProcessStartInfo {
                FileName = msiFilename,
                Arguments = "/passive",
            };

            SysProcess.Start(info);
        }
    }
}
