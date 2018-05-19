using System.Collections.Generic;

namespace Photon.Framework.Packages
{
    public class ApplicationPackageDefinition : IPackageDefinition
    {
        public string Id {get; set;}
        public string Name {get; set;}
        public string Description {get; set;}
        public List<PackageFileDefinition> Files {get; set;}


        public ApplicationPackageDefinition()
        {
            Files = new List<PackageFileDefinition>();
        }
    }
}
