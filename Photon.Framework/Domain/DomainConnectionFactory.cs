using Photon.Framework.Server;
using System;

namespace Photon.Framework.Domain
{
    public delegate DomainAgentSessionClient ConnectionRequestFunc(ServerAgent agent);

    public class DomainConnectionFactory : MarshalByRefObject
    {
        public event ConnectionRequestFunc OnConnectionRequest;


        public DomainAgentSessionClient RequestConnection(ServerAgent agent)
        {
            return OnConnectionRequest?.Invoke(agent);
        }
    }
}
