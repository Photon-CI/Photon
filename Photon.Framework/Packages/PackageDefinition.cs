using System.Collections.Generic;

namespace Photon.Framework.Packages
{
    public class PackageDefinition
    {
        public string Id {get; set;}
        public string Name {get; set;}
        public string Version {get; set;}
        public List<PackageFilter> Filters {get; set;}


        public PackageDefinition()
        {
            Filters = new List<PackageFilter>();
        }
    }
}
