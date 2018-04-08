using log4net;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.Messages;
using Photon.Server.Internal;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.MessageProcessors
{
    internal class ProjectPackageProcessor : MessageProcessorBase<ProjectPackageRequest>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProjectPackageProcessor));


        public override async Task<IResponseMessage> Process(ProjectPackageRequest requestMessage)
        {
            if (!PhotonServer.Instance.Sessions.TryGetSession(requestMessage.SessionId, out var session)) {
                Log.Warn($"Unable to map response, Server Session '{requestMessage.SessionId}' not found!");
                return null;
            }

            // TODO: Append ProjectPackage to session
            var filename = Path.Combine(session.WorkDirectory, "Project.zip");

            using (var fileStream = File.Open(filename, FileMode.Create, FileAccess.Write))
            using (var messageStream = requestMessage.StreamFunc()) {
                await messageStream.CopyToAsync(fileStream);
            }

            return new ProjectPackageResponse {
                Successful = true,
            };
        }
    }
}
