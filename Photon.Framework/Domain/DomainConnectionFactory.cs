using System;

namespace Photon.Framework.Domain
{
    public delegate DomainAgentSessionClient ConnectionRequestFunc(ServerAgentDefinition agent);

    public class DomainConnectionFactory : MarshalByRefObject
    {
        public event ConnectionRequestFunc OnConnectionRequest;


        public DomainAgentSessionClient RequestConnection(ServerAgentDefinition agent)
        {
            return OnConnectionRequest?.Invoke(agent);
        }
    }
}
