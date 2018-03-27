using Photon.Framework.Domain;
using System;

namespace Photon.Framework.Scripts
{
    internal class ScriptRegistry : TypeRegistry<IScript>
    {
        public ScriptResult ExecuteScript(ScriptContext context)
        {
            if (!map.TryGetValue(context.Job.Script, out var scriptClassType))
                throw new Exception($"Script '{context.Job.Script}' was not found!");

            object classObject = null;
            try {
                classObject = Activator.CreateInstance(scriptClassType);
                var script = classObject as IScript;

                return script.Run(context);
            }
            finally {
                (classObject as IDisposable)?.Dispose();
            }
        }
    }
}
