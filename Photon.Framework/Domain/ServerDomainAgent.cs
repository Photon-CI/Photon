using System.Threading;

namespace Photon.Framework.Domain
{
    public class ServerDomainAgent : DomainAgentBase
    {
        public void RunScript(string scriptName, string jsonData = null)
        {
            Thread.Sleep(6_000);
            //...
        }
    }
}
