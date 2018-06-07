using Photon.Framework.Server;

namespace Photon.Framework.Domain
{
    public delegate DomainAgentSessionClient ConnectionRequestFunc(ServerAgent agent);

    public class DomainConnectionFactory : MarshalByRefInstance
    {
        public event ConnectionRequestFunc OnConnectionRequest;


        public DomainAgentSessionClient RequestConnection(ServerAgent agent)
        {
            return OnConnectionRequest?.Invoke(agent);
        }
    }
}
