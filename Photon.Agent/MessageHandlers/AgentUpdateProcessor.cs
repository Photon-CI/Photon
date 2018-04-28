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

        private static async void Run(string msiFilename)
        {
            await Task.Run(async () => {
                await Task.Delay(200);

                var info = new ProcessStartInfo {
                    FileName = "msiexec.exe",
                    Arguments = $"/I \"{msiFilename}\" /passive /fe /L*V \"log.txt\"",
                };

                SysProcess.Start(info);
            });
        }
    }
}
