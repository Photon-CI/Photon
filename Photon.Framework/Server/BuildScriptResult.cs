using System;

namespace Photon.Framework.Server
{
    [Serializable]
    public class BuildScriptResult : ScriptResult
    {
        public int BuildNumber {get; set;}
    }
}
