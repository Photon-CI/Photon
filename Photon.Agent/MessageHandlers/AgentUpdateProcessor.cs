using log4net;
using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework;
using Photon.Library.TcpMessages;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class AgentUpdateProcessor : MessageProcessorBase<AgentUpdateRequest>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AgentUpdateProcessor));


        public override async Task<IResponseMessage> Process(AgentUpdateRequest requestMessage)
        {
            var updatePath = Path.Combine(Configuration.Directory, "Updates");
            var msiFilename = Path.Combine(updatePath, "Photon.Agent.msi");

            if (!Directory.Exists(updatePath))
                Directory.CreateDirectory(updatePath);

            if (File.Exists(msiFilename))
                File.Delete(msiFilename);

            File.Move(requestMessage.Filename, msiFilename);

            BeginInstall(updatePath, msiFilename);

            var response = new AgentUpdateResponse();

            return await Task.FromResult(response);
        }

        //private static void Run(string updatePath, string msiFilename)
        //{
        //    try {
        //        var cmd = $"msiexec.exe /i \"{msiFilename}\" /passive /l*vx \"log.txt\"";

        //        ProcessRunner.Run(updatePath, cmd);
        //    }
        //    catch (Exception error) {
        //        Log.Error("Failed to start agent update!", error);
        //    }
        //}

        private void BeginInstall(string updatePath, string msiFilename)
        {
            // TODO: Verify MSI?

            try {
                Program.Shutdown(); // TimeSpan.FromSeconds(20)
            }
            catch (Exception error) {
                Log.Error("An error occurred while shutting down!", error);
            }

            try {
                var cmd = $"msiexec.exe /i \"{msiFilename}\" /passive /l*vx \"log.txt\"";

                ProcessRunner.Run(updatePath, cmd);
            }
            catch (Exception error) {
                Log.Error("Failed to start agent update!", error);
            }
        }
    }
}
