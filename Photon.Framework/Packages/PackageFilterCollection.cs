using System.Collections.Generic;

namespace Photon.Framework.Packages
{
    public class PackageFilterCollection : List<PackageFilter>
    {
        public bool IncludesFile(string filename)
        {
            // TODO: Apply Filtering

            return true;
        }
    }
}
