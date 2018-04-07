using Photon.Framework.Projects;
using System;

namespace Photon.Framework.Tasks
{
    [Serializable]
    public class AgentBuildContext : IAgentBuildContext
    {
        public Project Project {get; set;}
        public string AssemblyFile {get; set;}
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public string WorkDirectory {get; set;}
        public ITaskOutput Output {get; set;}
    }
}
