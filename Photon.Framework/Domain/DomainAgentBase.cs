using System.Reflection;

namespace Photon.Framework.Domain
{
    public abstract class DomainAgentBase : MarshalByRefInstance
    {
        public virtual void LoadAssembly(string filename)
        {
            var assembly = Assembly.LoadFrom(filename);
            OnAssemblyLoaded(assembly);
        }

        protected virtual void OnAssemblyLoaded(Assembly assembly) {}
    }
}
