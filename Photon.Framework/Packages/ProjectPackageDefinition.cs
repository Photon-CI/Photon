using System.Collections.Generic;

namespace Photon.Framework.Packages
{
    public class ProjectPackageDefinition : IPackageDefinition
    {
        public string Id {get; set;}
        public string Name {get; set;}
        public string Description {get; set;}
        public string Assembly {get; set;}
        public string Script {get; set;}
        public List<PackageFileDefinition> Files {get; set;}


        public ProjectPackageDefinition()
        {
            Files = new List<PackageFileDefinition>();
        }
    }
}
