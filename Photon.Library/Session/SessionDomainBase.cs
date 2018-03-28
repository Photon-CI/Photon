using Photon.Framework.Domain;
using System;
using System.IO;
using System.Runtime.Remoting.Lifetime;
using System.Threading;

namespace Photon.Library.Session
{
    public abstract class SessionDomainBase<T> : IDisposable
        where T : DomainAgentBase
    {
        private AppDomain domain;
        private ClientSponsor sponsor;
        protected T agent;


        public void Dispose()
        {
            if (sponsor != null) {
                if (agent != null) {
                    //...
                    sponsor.Unregister(agent);
                }

                sponsor.Close();
                sponsor = null;
            }

            agent = null;

            if (domain != null) {
                try {
                    AppDomain.Unload(domain);
                }
                catch (ThreadAbortException) {}

                domain = null;
            }
        }

        public void Initialize(string assemblyFilename)
        {
            var assemblyName = Path.GetFileName(assemblyFilename);
            var assemblyPath = Path.GetDirectoryName(assemblyFilename);

            var domainSetup = new AppDomainSetup {
                ApplicationBase = assemblyPath,
                ConfigurationFile = $"{assemblyFilename}.config",
            };

            sponsor = new ClientSponsor();
            domain = AppDomain.CreateDomain(assemblyName, null, domainSetup);

            var agentType = typeof(T);
            agent = (T)domain.CreateInstanceAndUnwrap(agentType.Assembly.FullName, agentType.FullName);
            agent.LoadAssembly(assemblyFilename);

            sponsor.Register(agent);
        }
    }
}
