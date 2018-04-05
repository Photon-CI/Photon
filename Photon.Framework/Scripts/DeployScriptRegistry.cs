using Photon.Framework.Domain;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Scripts
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
                var script = classObject as IDeployScript;

                return await script.RunAsync(context);
            }
            finally {
                (classObject as IDisposable)?.Dispose();
            }
        }
    }
}
