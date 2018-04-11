using Photon.Framework.Domain;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Server
{
    internal class DeployScriptRegistry : TypeRegistry<IDeployScript>
    {
        public async Task<ScriptResult> ExecuteScript(IServerDeployContext context)
        {
            if (!map.TryGetValue(context.ScriptName, out var scriptClassType))
                throw new Exception($"Deploy Script '{context.ScriptName}' was not found!");

            object classObject = null;
            try {
                classObject = Activator.CreateInstance(scriptClassType);

                if (!(classObject is IDeployScript script)) throw new Exception($"Invalid IDeployScript implementation '{scriptClassType}'!");

                return await script.RunAsync(context);
            }
            finally {
                (classObject as IDisposable)?.Dispose();
            }
        }
    }
}
