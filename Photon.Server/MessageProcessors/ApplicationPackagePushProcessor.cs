using log4net;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Domain;
using Photon.Framework.Tools;
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
            try {
                if (!PhotonServer.Instance.Sessions.TryGet(requestMessage.ServerSessionId, out var session))
                    throw new Exception($"Agent Session ID '{requestMessage.ServerSessionId}' not found!");

                await RemoteTaskCompletionSource.Run(taskHandle => {
                    session.Packages.Client.PushApplicationPackage(requestMessage.Filename, taskHandle);
                });
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
