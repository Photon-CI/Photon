using System;
using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using log4net;
using SysProcess = System.Diagnostics.Process;

namespace Photon.Agent.MessageHandlers
{
    public class AgentUpdateProcessor : MessageProcessorBase<AgentUpdateRequest>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AgentUpdateProcessor));


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
            try {
                var info = new ProcessStartInfo {
                    FileName = "msiexec.exe",
                    Arguments = $"/i \"{msiFilename}\" /passive /l*vx \"log.txt\"",
                };

                SysProcess.Start(info);
            }
            catch (Exception error) {
                Log.Error("Failed to start installation file!", error);
            }

            //await Task.Run(() => {
                //await Task.Delay(200);

            //});
        }
    }
}
