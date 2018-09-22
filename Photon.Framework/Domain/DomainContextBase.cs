using Photon.Framework.Artifacts;
using Photon.Framework.Packages;
using Photon.Framework.Process;
using Photon.Framework.Projects;
using Photon.Framework.Variables;
using System;

namespace Photon.Framework.Domain
{
    [Serializable]
    public abstract class DomainContextBase : IDomainContext
    {
        public string ServerSessionId {get; set;}
        public Project Project {get; set;}
        public string AssemblyFilename {get; set;}
        public string WorkDirectory {get; set;}
        public string BinDirectory {get; set;}
        public string ContentDirectory {get; set;}
        public DomainOutput Output {get; set;}
        public IPackageClient Packages {get; set;}
        public VariableSetCollection ServerVariables {get; set;}
        public VariableSetCollection AgentVariables {get; set;}
        public ArtifactManagerClient Artifacts {get; set;}
        public ProcessClient Process {get; set;}


        protected DomainContextBase()
        {
            Process = new ProcessClient(this);
        }
    }
}
