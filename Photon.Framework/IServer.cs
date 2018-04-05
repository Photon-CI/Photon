using Photon.Framework.Scripts;
using System.Collections.Generic;

namespace Photon.Framework
{
    public interface IServer
    {
        string WorkPath {get;}
        IEnumerable<BuildScriptAgent> GetAgents(params string[] roles);
    }
}
