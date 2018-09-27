using log4net;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Packages;
using Photon.Library.Packages;
using Photon.Library.TcpMessages.Packages;
using Photon.Server.Internal;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.MessageProcessors
{
    internal class ProjectPackagePushProcessor : MessageProcessorBase<AgentProjectPackagePushRequest>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProjectPackagePushProcessor));


        public override async Task<IResponseMessage> Process(AgentProjectPackagePushRequest requestMessage)
        {
            var serverContext = PhotonServer.Instance.Context;

            try {
                if (!serverContext.Sessions.TryGet(requestMessage.ServerSessionId, out var session))
                    throw new Exception($"Agent Session ID '{requestMessage.ServerSessionId}' not found!");

                var metadata = await ProjectPackageTools.GetMetadataAsync(requestMessage.Filename);
                if (metadata == null) throw new ApplicationException($"Invalid Project Package '{requestMessage.Filename}'! No metadata found.");

                await serverContext.ProjectPackages.Add(requestMessage.Filename);
                session.PushedProjectPackages.Add(new PackageReference(metadata.Id, metadata.Version));
            }
            finally {
                try {
                    File.Delete(requestMessage.Filename);
                }
                catch (Exception error) {
                    Log.Warn("Failed to remove temporary project package!", error);
                }
            }

            return new ResponseMessageBase();
        }
    }
}
