using System.Collections.Generic;

namespace Photon.Framework.Packages
{
    public class PackageFilterCollection : List<PackageFileDefinition>
    {
        public bool IncludesFile(string filename)
        {
            // TODO: Apply Filtering

            return true;
        }
    }
}
