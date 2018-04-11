using Photon.Framework.Domain;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    internal class BuildScriptRegistry : TypeRegistry<IBuildScript>
    {
        public async Task<ScriptResult> ExecuteScript(IServerBuildContext context)
        {
            if (!map.TryGetValue(context.ScriptName, out var scriptClassType))
                throw new Exception($"Build Script '{context.ScriptName}' was not found!");

            object classObject = null;
            try {
                classObject = Activator.CreateInstance(scriptClassType);
                var script = classObject as IBuildScript;

                if (script == null) throw new Exception($"Unable to run Build-Script of type '{scriptClassType}'!");

                return await script.RunAsync(context);
            }
            finally {
                (classObject as IDisposable)?.Dispose();
            }
        }
    }
}
