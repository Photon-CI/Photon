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
            try {
                Assembly.LoadFrom(filename);
            }
            catch (Exception error) {
                var e = new ApplicationException($"Failed to load assembly '{filename}'!");
                e.Data["source-exception"] = error.ToString();
                throw e;
            }
            //OnAssemblyLoaded(assembly);
        }

        protected virtual void OnAssemblyLoaded(Assembly assembly)
        {
            //...
        }

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
