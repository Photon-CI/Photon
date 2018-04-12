using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Server.Internal;
using System.IO;
using System.Threading.Tasks;
using Photon.Library.TcpMessages;

namespace Photon.Server.MessageProcessors
{
    internal class ApplicationPackagePushProcessor : MessageProcessorBase<ApplicationPackagePushRequest>
    {
        public override async Task<IResponseMessage> Process(ApplicationPackagePushRequest requestMessage)
        {
            //if (!(Transceiver.Context is IServerSession session))
            //    throw new Exception("Session is undefined!");

            try {
                await PhotonServer.Instance.ApplicationPackages.Add(requestMessage.Filename);
            }
            finally {
                try {File.Delete(requestMessage.Filename);}
                catch {}
            }

            return new ApplicationPackagePushResponse();
        }
    }
}
