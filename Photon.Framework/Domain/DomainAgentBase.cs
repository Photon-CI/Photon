using System;
using System.Reflection;

namespace Photon.Framework.Domain
{
    public abstract class DomainAgentBase : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            // TODO: This is an awful hack, but I've been
            // TODO: unable to get sponsorship working properly.
            return null;
        }

        public virtual void LoadAssembly(string filename)
        {
            var assembly = Assembly.LoadFrom(filename);
            OnAssemblyLoaded(assembly);
        }

        protected virtual void OnAssemblyLoaded(Assembly assembly) {}
    }
}
