using System.Collections.Generic;

namespace Photon.Framework.Packages
{
    public class PackageFileDefinition
    {
        public string Path {get; set;}

        public string Destination {get; set;}

        public List<string> Exclude {get; set;}
    }
}
