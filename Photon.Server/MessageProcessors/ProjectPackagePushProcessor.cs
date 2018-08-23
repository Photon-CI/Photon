using log4net;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Domain;
using Photon.Library.TcpMessages;
using Photon.Server.Internal;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.MessageProcessors
{
    internal class ProjectPackagePushProcessor : MessageProcessorBase<ProjectPackagePushRequest>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProjectPackagePushProcessor));


        public override async Task<IResponseMessage> Process(ProjectPackagePushRequest requestMessage)
        {
            try {
                if (!PhotonServer.Instance.Sessions.TryGet(requestMessage.ServerSessionId, out var session))
                    throw new Exception($"Agent Session ID '{requestMessage.ServerSessionId}' not found!");

                await RemoteTaskCompletionSource.Run(taskHandle => {
                    session.Packages.Client.PushProjectPackage(requestMessage.Filename, taskHandle);
                });
            }
            finally {
                try {
                    File.Delete(requestMessage.Filename);
                }
                catch (Exception error) {
                    Log.Warn("Failed to remove temporary project package!", error);
                }
            }

            return new ProjectPackagePushResponse();
        }
    }
}
