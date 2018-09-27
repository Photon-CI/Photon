using log4net;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Packages;
using Photon.Framework.Tools;
using Photon.Library.Packages;
using Photon.Library.TcpMessages;
using Photon.Server.Internal;
using System;
using System.Threading.Tasks;

namespace Photon.Server.MessageProcessors
{
    internal class ApplicationPackagePushProcessor : MessageProcessorBase<ApplicationPackagePushRequest>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ApplicationPackagePushProcessor));


        public override async Task<IResponseMessage> Process(ApplicationPackagePushRequest requestMessage)
        {
            var serverContext = PhotonServer.Instance.Context;

            try {
                if (!serverContext.Sessions.TryGet(requestMessage.ServerSessionId, out var session))
                    throw new Exception($"Session ID '{requestMessage.ServerSessionId}' not found!");

                var metadata = await ApplicationPackageTools.GetMetadataAsync(requestMessage.Filename);
                if (metadata == null) throw new ApplicationException($"Invalid Project Package '{requestMessage.Filename}'! No metadata found.");

                await serverContext.ApplicationPackages.Add(requestMessage.Filename);
                session.PushedApplicationPackages.Add(new PackageReference(metadata.Id, metadata.Version));
            }
            finally {
                try {
                    PathEx.Delete(requestMessage.Filename);
                }
                catch (Exception error) {
                    Log.Warn("Failed to remove temporary project package!", error);
                }
            }

            return new ApplicationPackagePushResponse();
        }
    }
}
