using System.Collections.Generic;

namespace Photon.Framework.Packages
{
    public class PackageFilter
    {
        public string Include {get; set;}
        public List<string> Exclude {get; set;}
    }
}
