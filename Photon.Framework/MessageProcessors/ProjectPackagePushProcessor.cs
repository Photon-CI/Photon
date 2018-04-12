using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Server;
using Photon.Framework.TcpMessages;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Framework.MessageProcessors
{
    internal class ProjectPackagePushProcessor : MessageProcessorBase<ProjectPackagePushRequest>
    {
        public override async Task<IResponseMessage> Process(ProjectPackagePushRequest requestMessage)
        {
            if (!(Transceiver.Context is IServerContext sessionContext))
                throw new Exception("Server Context is undefined!");

            try {
                await sessionContext.ProjectPackages.Add(requestMessage.Filename);
            }
            finally {
                try {File.Delete(requestMessage.Filename);}
                catch {}
            }

            return new ProjectPackagePushResponse();
        }
    }
}
