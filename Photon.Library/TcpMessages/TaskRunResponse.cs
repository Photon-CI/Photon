using Photon.Communication.Messages;
using Photon.Framework.Tasks;

namespace Photon.Library.TcpMessages
{
    public class TaskRunResponse : ResponseMessageBase
    {
        public TaskResult Result {get; set;}
    }
}
