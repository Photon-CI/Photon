using System;
using System.Reflection;

namespace Photon.Framework.Domain
{
    public abstract class DomainAgentBase : MarshalByRefObject
    {
        public DomainAgentBase()
        {
            AppDomain.CurrentDomain.AssemblyResolve += Domain_OnAssemblyResolve;
        }

        public virtual void LoadAssembly(string filename)
        {
            var assembly = Assembly.LoadFrom(filename);
            OnAssemblyLoaded(assembly);
        }

        protected virtual void OnAssemblyLoaded(Assembly assembly) {}

        private Assembly Domain_OnAssemblyResolve(object sender, ResolveEventArgs e)
        {
            Assembly _assembly = null;

            try {
                _assembly = Assembly.Load(e.Name);
            }
            catch {}

            return _assembly;
        }
    }
}
