using log4net;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.Messages;
using Photon.Server.Internal;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.MessageProcessors
{
    internal class ProjectPackageProcessor : MessageProcessorBase<ProjectPackageRequest>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProjectPackageProcessor));


        public override async Task<IResponseMessage> Process(ProjectPackageRequest requestMessage)
        {
            var response = new ProjectPackageResponse();

            try {
                await PhotonServer.Instance.ProjectPackages.Add(requestMessage.Filename);

                response.Successful = true;
            }
            catch (Exception error) {
                Log.Error("Failed to add Project Package!", error);
                response.Exception = error.Message;
            }

            try {
                File.Delete(requestMessage.Filename);
            }
            catch (Exception error) {
                Log.Warn("Failed to remove temporary package file!", error);
            }

            return response;
        }
    }
}
