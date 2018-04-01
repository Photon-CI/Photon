using Photon.Framework.Scripts;
using System.Collections.Generic;

namespace Photon.Framework
{
    public interface IServer
    {
        string WorkDirectory {get;}
        IEnumerable<ScriptAgent> GetAgents(params string[] roles);
    }
}
