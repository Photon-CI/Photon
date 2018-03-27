using Photon.Framework.Domain;
using System;
using System.IO;
using System.Runtime.Remoting.Lifetime;

namespace Photon.Library.Session
{
    public abstract class SessionDomainBase<T> : IDisposable
        where T : DomainAgentBase
    {
        private AppDomain domain;
        private ClientSponsor sponsor;

        protected T agent;


        public SessionDomainBase()
        {
            //
        }

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
                AppDomain.Unload(domain);
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

            try {
                agent.LoadAssembly(assemblyFilename);
            }
            catch (Exception error) {
                throw error;
            }

            sponsor.Register(agent);
        }
    }
}
