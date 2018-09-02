using log4net;
using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Process;
using Photon.Framework.Tools;
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

            PathEx.CreatePath(updatePath);

            if (File.Exists(msiFilename))
                File.Delete(msiFilename);

            File.Move(requestMessage.Filename, msiFilename);

            var _ = Task.Delay(100)
                .ContinueWith(async t => {
                    await BeginInstall(updatePath, msiFilename);
                });

            var response = new AgentUpdateResponse();

            return await Task.FromResult(response);
        }

        private async Task BeginInstall(string updatePath, string msiFilename)
        {
            // TODO: Verify MSI?

            try {
                await PhotonAgent.Instance.Shutdown(TimeSpan.FromSeconds(30));
            }
            catch (Exception error) {
                Log.Error("An error occurred while shutting down!", error);
            }

            Log.Debug("Starting agent update...");

            try {
                var runInfo = new ProcessRunInfo {
                    Filename = "msiexec.exe",
                    Arguments = $"/i \"{msiFilename}\" /passive /l*vx \"log.txt\"",
                    WorkingDirectory = updatePath,
                };

                using (var process = new ProcessRunner().Start(runInfo)) {
                    if (process == null)
                        throw new ApplicationException("Failed to start process!");
                }

                Log.Info("Agent update started.");
            }
            catch (Exception error) {
                Log.Error("Failed to start agent update!", error);
            }
        }
    }
}
