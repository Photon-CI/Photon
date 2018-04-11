using Photon.Communication.Messages;
using Photon.Framework.Tasks;

namespace Photon.Framework.TcpMessages
{
    public class BuildTaskRunResponse : ResponseMessageBase
    {
        public TaskResult Result {get; set;}
    }
}
